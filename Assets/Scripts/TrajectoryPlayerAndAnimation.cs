using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPlayerAndAnimation : MonoBehaviour {
    
    private Transform chBody;
    private Transform trajectPath;

    public GameObject frPointPref;
    public GameObject jPref;
    public float phase;

    [Header("Joints")]
    public int jNumber = 31;
    
    public float strfAmnt;
    public float strfTrgt;
    public float crAmnt;
    public float crTrgt;
    public float respons;

    public struct BodyComponents {
        public Vector3 pos;
        public Vector3 vel;
        public Quaternion rot;

        public GameObject jPnt;
    }
    public BodyComponents[] bodies;


    [Header("Trajectory")]
    [Range(0.0f, 1.0f)]
    public float scalFact = 0.04f;
    //противоположный размер
    private float oppoScFact;

    public int nOfTrajProject = 12;
    public int trajLen;

    [Tooltip("Расстояние справа и слева от средней точки траектории!")]
    public float sidePointsOffset = 25.0f;

    [Tooltip("Индекс слоя, который следует игнорировать при измерении высоты!")]
    public int layerVisor;
    public float hghtOriginaly;

    private Vector3 goalDir;
    private Vector3 goalVel;    

    public struct TrajectParametrs {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 dir;
        public float hght;
        public float walkStand;
        public float walkWalk;
        public float walkJog;
        public float walkCrouch;
        public float walkJump;
        public float walkBump;

        public GameObject frPoint;
    }

    public TrajectParametrs[] p;

    private Utillites.WallPoints[] worldWalls;
    public float wallWidth = 1.5f;
    public float wallVal = 1.1f;

    // Начальная инициализация
    void Start () {

        ReturnWalls();

        chBody = gameObject.transform.GetChild(1);
        InitialBody();

        trajectPath = gameObject.transform.GetChild(2);
        InitialTraject();

        layerVisor = 1 << layerVisor;
        layerVisor = ~layerVisor;

        oppoScFact = 1 / scalFact;
    }

    private void ReturnWalls() {

        Transform worldWalls = GameObject.Find("WorldWalls").transform;
        this.worldWalls = new Utillites.WallPoints[worldWalls.childCount];


        for (int i = 0; i < worldWalls.childCount; i++) {
            this.worldWalls[i] = worldWalls.GetChild(i).GetComponent<WallScript>().GetWallPoints();
        }
    }
    // инициализируем перса
    private void InitialBody() {

        phase = 0.0f;

        strfAmnt = 0.0f;
        strfTrgt = 0.0f;
        crAmnt = 0.0f;
        crTrgt = 0.0f;
        respons = 0.0f;

        bodies = new BodyComponents[jNumber];

        InitialBodyElements();
    }
    //Инициализация составляющих персонажа
    private void InitialBodyElements() {

        for (int i = 0; i < jNumber; i++) {
            var newJoint = Instantiate(
                jPref,
                new Vector3(
                    transform.position.x,
                    0.5f,
                    transform.position.z),
                Quaternion.identity,
                chBody);
            newJoint.name = "Joint_" + i;
            
            bodies[i].jPnt = newJoint;
        }
    }

    private void InitialTraject() {

        trajLen = nOfTrajProject * 10;
        p = new TrajectParametrs[trajLen];

        goalDir = Vector3.forward;
        goalVel = new Vector3();

        InitialTrajectPoints();
    }

    private void InitialTrajectPoints() {

        for (int i = 0; i < trajLen; i += 10) {
            var newPoint = Instantiate(
                frPointPref,
                new Vector3(
                    transform.position.x,
                    transform.position.y,
                    // временное фиг значение
                    (transform.position.z + (-6f + (i / 10)))),                      
                Quaternion.identity,
                trajectPath);
            newPoint.name = "FramePoint_" + (i / 10);

            p[i].frPoint = newPoint;
        }
    }
    //сброс позиции персонажа на точку (0f,0f,0f)
    public void ResetPosition(Vector3 initialPosition, Mtrx Y) {

        Vector3 rootPos = new Vector3(
            initialPosition.x,
            ReturnHghtSample(initialPosition),
            initialPosition.z);
        Quaternion rootRotation = new Quaternion();

        for (int i = 0; i < jNumber; i++) {
            int oPosition = 8 + (((trajLen / 2) / 10) * 4) + (jNumber * 3 * 0);
            int oVelocity = 8 + (((trajLen / 2) / 10) * 4) + (jNumber * 3 * 1);
            int oRotation = 8 + (((trajLen / 2) / 10) * 4) + (jNumber * 3 * 2);

            Vector3 pos = rootRotation
                * new Vector3(
                Y[oPosition + i * 3 + 0],
                Y[oPosition + i * 3 + 1],
                Y[oPosition + i * 3 + 2])
                + rootPos;
            Vector3 vel = rootRotation
                * new Vector3(
                Y[oVelocity + i * 3 + 0],
                Y[oVelocity + i * 3 + 1],
                Y[oVelocity + i * 3 + 2]);
            Quaternion rot = rootRotation
                * Utillites.QuaterExp(new Vector3(                                                            
                    Y[oRotation + i * 3 + 0],
                    Y[oRotation + i * 3 + 1],
                    Y[oRotation + i * 3 + 2]));

            bodies[i].pos = pos;
            bodies[i].vel = vel;
            bodies[i].rot = rot;
        }

        for (int i = 0; i < trajLen; i++) {
            p[i].pos = rootPos;
            p[i].rot = rootRotation;
            p[i].dir = Vector3.forward;
            p[i].hght = rootPos.y;
            p[i].walkStand = 0.0f;
            p[i].walkWalk = 0.0f;
            p[i].walkJog = 0.0f;
            p[i].walkCrouch = 0.0f;
            p[i].walkJump = 0.0f;
            p[i].walkBump = 0.0f;
        }

        phase = 0.0f;
    }

    public void UpdateStrafe(float leftTrigger) {

        strfTrgt = leftTrigger;
        strfAmnt = Mathf.Lerp(strfAmnt, strfTrgt, Utillites.exStrafSmth);
    }

    public void UpdateGoalDirAndVel(Vector3 newTargetDirection, float axisX, float axisY, float rightTrigger) {

        Quaternion newTargetRotation = Quaternion.AngleAxis(
            (Mathf.Atan2(newTargetDirection.x, newTargetDirection.z) * Mathf.Rad2Deg),
            Vector3.up);
        
        float movementSpeed = 2.5f + 2.5f * rightTrigger;

        Vector3 newTargetVelocity = movementSpeed * (newTargetRotation * (new Vector3(axisX, 0.0f, axisY)));
        goalVel = Vector3.Lerp(goalVel, newTargetVelocity, Utillites.exVelSmth);
        
        Vector3 targetVelocityDirection = goalVel.magnitude < 1e-05 ? goalDir : goalVel.normalized;

        newTargetDirection = Utillites.MixDir(targetVelocityDirection, newTargetDirection, strfAmnt);
        goalDir = Utillites.MixDir(goalDir, newTargetDirection, Utillites.exDirSmth);

        crAmnt = Mathf.Lerp(crAmnt, crTrgt, Utillites.exCrouchedSmth);

        Debug.DrawRay(this.transform.position, goalDir * 10, Color.red);
        Debug.DrawRay(this.transform.position, targetVelocityDirection * 10, Color.green);
    }

    public void UpWalk(float rightTrigger) {

        if (goalVel.magnitude < 0.1f) { // стоять на месте
            float standAmount = 1.0f - Mathf.Clamp01(goalVel.magnitude / 0.1f);
            
            p[trajLen / 2].walkStand  = Mathf.Lerp(p[trajLen / 2].walkStand, standAmount, Utillites.exGaitSmth);
            p[trajLen / 2].walkWalk   = Mathf.Lerp(p[trajLen / 2].walkWalk, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkJog    = Mathf.Lerp(p[trajLen / 2].walkJog, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkCrouch = Mathf.Lerp(p[trajLen / 2].walkCrouch, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkJump   = Mathf.Lerp(p[trajLen / 2].walkJump, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkBump   = Mathf.Lerp(p[trajLen / 2].walkBump, 0.0f, Utillites.exGaitSmth);

        } else if (crAmnt > 0.1f) { // замедление
            p[trajLen / 2].walkStand  = Mathf.Lerp(p[trajLen / 2].walkStand, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkWalk   = Mathf.Lerp(p[trajLen / 2].walkWalk, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkJog    = Mathf.Lerp(p[trajLen / 2].walkJog, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkCrouch = Mathf.Lerp(p[trajLen / 2].walkCrouch, crAmnt, Utillites.exGaitSmth);
            p[trajLen / 2].walkJump   = Mathf.Lerp(p[trajLen / 2].walkJump, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkBump   = Mathf.Lerp(p[trajLen / 2].walkBump, 0.0f, Utillites.exGaitSmth);

        } else if (rightTrigger != 0.0f) { 
            p[trajLen / 2].walkStand  = Mathf.Lerp(p[trajLen / 2].walkStand, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkWalk   = Mathf.Lerp(p[trajLen / 2].walkWalk, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkJog    = Mathf.Lerp(p[trajLen / 2].walkJog, 1.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkCrouch = Mathf.Lerp(p[trajLen / 2].walkCrouch, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkJump   = Mathf.Lerp(p[trajLen / 2].walkJump, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkBump   = Mathf.Lerp(p[trajLen / 2].walkBump, 0.0f, Utillites.exGaitSmth);

        } else {
            // ---Ходьба---
            p[trajLen / 2].walkStand  = Mathf.Lerp(p[trajLen / 2].walkStand, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkWalk   = Mathf.Lerp(p[trajLen / 2].walkWalk, 1.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkJog    = Mathf.Lerp(p[trajLen / 2].walkJog, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkCrouch = Mathf.Lerp(p[trajLen / 2].walkCrouch, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkJump   = Mathf.Lerp(p[trajLen / 2].walkJump, 0.0f, Utillites.exGaitSmth);
            p[trajLen / 2].walkBump   = Mathf.Lerp(p[trajLen / 2].walkBump, 0.0f, Utillites.exGaitSmth);
        }
    }

    public void ComputeFutureTraject() {

        Vector3[] posBlend = new Vector3[trajLen];
        posBlend[trajLen / 2] = p[trajLen / 2].pos;

        for (int i = ((trajLen / 2) + 1); i < trajLen; i++) {
            float biasPos = Mathf.Lerp(0.5f, 1.0f, strfAmnt);                                       
            float biasDir = Mathf.Lerp(2.0f, 0.5f, strfAmnt);
            // c 569
            float scalePos = 1.0f - Mathf.Pow((1.0f - ((float)(i - trajLen / 2) / (trajLen / 2))), biasPos);
            float scaleDir = 1.0f - Mathf.Pow((1.0f - ((float)(i - trajLen / 2) / (trajLen / 2))), biasDir);

            posBlend[i] = posBlend[i - 1] + Vector3.Lerp(
                p[i].pos - p[i - 1].pos,
                goalVel,
                scalePos);

            
            for (int j = 0; j < worldWalls.Length; j++) {
                Vector2 trajPoint = new Vector2(posBlend[i].x * scalFact, posBlend[i].z * scalFact);  // столкновения со стенами

                if ((trajPoint - ((worldWalls[j].wallStart + worldWalls[j].wallEnd) / 2.0f)).magnitude >
                    (worldWalls[j].wallStart - worldWalls[j].wallEnd).magnitude) {
                    continue;
                }

                Vector2 segPoint = Utillites.SegNearest(worldWalls[j].wallStart, worldWalls[j].wallEnd, trajPoint);
                float segDist = (segPoint - trajPoint).magnitude;

                if (segDist < (wallWidth + wallVal)) {
                    Vector2 p0 = (wallWidth + 0.0f) * (trajPoint - segPoint).normalized + segPoint;
                    Vector2 p1 = (wallWidth + wallVal) * (trajPoint - segPoint).normalized + segPoint;
                    Vector2 p = Vector2.Lerp(p0, p1, Mathf.Clamp01(segDist - wallWidth) / wallVal);

                    posBlend[i].x = p.x * oppoScFact;
                    posBlend[i].z = p.y * oppoScFact;
                }
            }

            p[i].dir = Utillites.MixDir(p[i].dir, goalDir, scaleDir);

            p[i].hght = p[trajLen/ 2].hght;

            p[i].walkStand  = p[trajLen / 2].walkStand;
            p[i].walkWalk   = p[trajLen / 2].walkWalk;
            p[i].walkJog    = p[trajLen / 2].walkJog;
            p[i].walkCrouch = p[trajLen / 2].walkCrouch;
            p[i].walkJump   = p[trajLen / 2].walkJump;
            p[i].walkBump   = p[trajLen / 2].walkBump;
        }

        for (int i = ((trajLen / 2) + 1); i < trajLen; i++) {
            p[i].pos = posBlend[i];
        }        
    }

    public void Jumps() {

        for (int i = ((trajLen / 2) + 1); i < trajLen; i++) {
            p[i].walkJump = 0.0f;

            p[i].walkJump = Mathf.Max(
                p[i].walkJump,
                1.0f - Mathf.Clamp01( (3.0f / 5.0f))
                );
        }
    }

    public void Walls() {

        for (int i = 0; i < trajLen; i++) {
            p[i].walkBump = 0.0f;
            for (int j = 0; j < worldWalls.Length; j++) {
                Vector2 trajectPoint = new Vector2(p[i].pos.x * scalFact, p[i].pos.z * scalFact);
                Vector2 segPoint = Utillites.SegNearest(worldWalls[j].wallStart, worldWalls[j].wallEnd, trajectPoint);

                float segmentDistance = (segPoint - trajectPoint).magnitude;
                p[i].walkBump = Mathf.Max(p[i].walkBump, 1.0f - Mathf.Clamp01((segmentDistance - wallWidth) / wallVal));
            }
        }
    }

    public void UpRot() {

        for (int i = 0; i < trajLen; i++) {
            p[i].rot = Quaternion.AngleAxis(
                (Mathf.Atan2(p[i].dir.x, p[i].dir.z) * Mathf.Rad2Deg),
                Vector3.up);
        }
    }

    public void UpHghts() {

        for (int i = (trajLen / 2); i < trajLen; i++) {
            p[i].pos.y = ReturnHghtSample(p[i].pos);
        }

        p[trajLen / 2].hght = 0.0f;
        for (int i = 0; i < trajLen; i += 10) {
            p[trajLen / 2].hght += (p[i].pos.y / (trajLen / 10));
        }
    }

    public float ReturnHghtSample(Vector3 position) {
        
        RaycastHit hit;
        position.Scale(new Vector3(scalFact, 0.0f, scalFact));    
        position.y = hghtOriginaly;

        if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, layerVisor)) {
            if (hit.transform.tag == "Terrain") {
                return (hghtOriginaly - hit.distance) * oppoScFact;
            }            
        }        
        return 0.0f;
    }

    public void UpNetInput(ref Mtrx X) {

        Vector3 rootPos = new Vector3(
            p[trajLen / 2].pos.x,
            p[trajLen / 2].hght,
            p[trajLen / 2].pos.z);

        Quaternion rootRotation = p[trajLen / 2].rot;

        int w = trajLen / 10;

        // Траектория положения в пространстве и направления
        for (int i = 0; i < trajLen; i += 10) {
            Vector3 position = Quaternion.Inverse(rootRotation) * (p[i].pos - rootPos);
            Vector3 direction = Quaternion.Inverse(rootRotation) * p[i].dir;

            X[(w * 0) + (i / 10)] = position.x;
            X[(w * 1) + (i / 10)] = position.z;

            X[(w * 2) + (i / 10)] = direction.x;
            X[(w * 3) + (i / 10)] = direction.z;
        }

        // Тректория походок
        for (int i = 0; i < trajLen; i += 10) {
            X[(w * 4) + (i / 10)] = p[i].walkStand;
            X[(w * 5) + (i / 10)] = p[i].walkWalk;
            X[(w * 6) + (i / 10)] = p[i].walkJog;
            X[(w * 7) + (i / 10)] = p[i].walkCrouch;
            X[(w * 8) + (i / 10)] = p[i].walkJump;
            X[(w * 9) + (i / 10)] = 0.0f;
        }

        // Совмещенное предыщее положение, скорость, вращение
        Vector3 previousRootPosition = new Vector3(
            p[(trajLen / 2) - 1].pos.x,
            p[(trajLen / 2) - 1].hght,
            p[(trajLen / 2) - 1].pos.z);

        Quaternion prevRootRot = p[(trajLen / 2) - 1].rot;

        int o = (trajLen / 10) * 10;
        for (int i = 0; i < jNumber; i++) {
            Vector3 pos = Quaternion.Inverse(prevRootRot) * (bodies[i].pos - previousRootPosition);
            Vector3 prv = Quaternion.Inverse(prevRootRot) * bodies[i].vel;

            X[o + (jNumber * 3 * 0) + (i * 3 + 0)] = pos.x;
            X[o + (jNumber * 3 * 0) + (i * 3 + 1)] = pos.y;
            X[o + (jNumber * 3 * 0) + (i * 3 + 2)] = pos.z;

            X[o + (jNumber * 3 * 1) + (i * 3 + 0)] = prv.x;
            X[o + (jNumber * 3 * 1) + (i * 3 + 1)] = prv.y;
            X[o + (jNumber * 3 * 1) + (i * 3 + 2)] = prv.z;            
        }

        // Траектории высот
        o += (jNumber * 3 * 2);
        for (int i = 0; i < trajLen; i += 10) {
            Vector3 positionRight = p[i].pos + (p[i].rot * new Vector3(sidePointsOffset, 0.0f, 0.0f));
            Vector3 positionLeft  = p[i].pos + (p[i].rot * new Vector3(-sidePointsOffset, 0.0f, 0.0f));

            X[o + (w * 0) + (i / 10)] = ReturnHghtSample(positionRight) - rootPos.y;
            X[o + (w * 1) + (i / 10)] = p[i].pos.y - rootPos.y;
            X[o + (w * 2) + (i / 10)] = ReturnHghtSample(positionLeft) - rootPos.y;
        }
    }

    public void CreateLocalTrans(Mtrx Y) {

        Vector3 rootPos = new Vector3(
            p[trajLen / 2].pos.x,
            p[trajLen / 2].hght,
            p[trajLen / 2].pos.z);

        Quaternion rootRot = p[trajLen / 2].rot;

        for (int i = 0; i < jNumber; i++) {
            int oPosition = 8 + (((trajLen / 2) / 10) * 4) + (jNumber * 3 * 0);
            int oVelocity = 8 + (((trajLen / 2) / 10) * 4) + (jNumber * 3 * 1);
            int oRotation = 8 + (((trajLen / 2) / 10) * 4) + (jNumber * 3 * 2);
            
            Vector3 position = rootRot
                * new Vector3(
                Y[oPosition + i * 3 + 0],
                Y[oPosition + i * 3 + 1],
                Y[oPosition + i * 3 + 2])
                + rootPos;
            Vector3 vel = rootRot
                * new Vector3(
                Y[oVelocity + i * 3 + 0],
                Y[oVelocity + i * 3 + 1],
                Y[oVelocity + i * 3 + 2]);
            Quaternion rot = rootRot
                * Utillites.QuaterExp(                                                                  
                new Vector3(
                    Y[oRotation + i * 3 + 0],
                    Y[oRotation + i * 3 + 1],
                    Y[oRotation + i * 3 + 2]));
            // return 1705 - 2022
            bodies[i].pos = Vector3.Lerp((bodies[i].pos + vel), position, Utillites.exBodiesSmth);
            bodies[i].vel = vel;
            bodies[i].rot = rot;

           
        }
    }

    public void PostVisualCompute(Mtrx Y) {

        // Обновление предыдущей траектории
        for (int i = 0; i < (trajLen / 2); i++) {
            p[i].pos   = p[i + 1].pos;
            p[i].rot   = p[i + 1].rot;
            p[i].dir  = p[i + 1].dir;
            p[i].hght     = p[i + 1].hght;
            p[i].walkStand  = p[i + 1].walkStand;
            p[i].walkWalk   = p[i + 1].walkWalk;
            p[i].walkJog    = p[i + 1].walkJog;
            p[i].walkCrouch = p[i + 1].walkCrouch;
            p[i].walkJump   = p[i + 1].walkJump;
            p[i].walkBump   = p[i + 1].walkBump;
        }

        // Обновление текущей траектории
        float stAmnt = ReturnStAmnt();

        Vector3 trajectUp = p[trajLen / 2].rot * new Vector3(Y[0], 0.0f, Y[1]);
        p[trajLen / 2].pos = p[trajLen / 2].pos + stAmnt * trajectUp;
        
        p[trajLen / 2].dir = Quaternion.AngleAxis(
            ((stAmnt * -Y[2, 0]) * Mathf.Rad2Deg), Vector3.up) * p[trajLen / 2].dir;

        p[trajLen / 2].rot = Quaternion.AngleAxis(
                (Mathf.Atan2(p[trajLen / 2].dir.x, p[trajLen / 2].dir.z) * Mathf.Rad2Deg),
                Vector3.up);

        // столкновения со стеной
        for (int j = 0; j < worldWalls.Length; j++) {
            Vector2 trajectoryPoint = new Vector2(p[trajLen / 2].pos.x * scalFact, p[trajLen / 2].pos.z * scalFact);
            Vector2 segmentPoint = Utillites.SegNearest(worldWalls[j].wallStart, worldWalls[j].wallEnd, trajectoryPoint);

            float segmentDistance = (segmentPoint - trajectoryPoint).magnitude;

            if (segmentDistance < (wallWidth + wallVal)) {
                Vector2 point0 = (wallWidth + 0.0f) * (trajectoryPoint - segmentPoint).normalized + segmentPoint;
                Vector2 point1 = (wallWidth + wallVal) * (trajectoryPoint - segmentPoint).normalized + segmentPoint;
                Vector2 point = Vector2.Lerp(point0, point1, Mathf.Clamp01((segmentDistance - wallWidth) / wallVal));

                p[trajLen / 2].pos.x = point.x * oppoScFact;
                p[trajLen / 2].pos.z = point.y * oppoScFact;
            }
        }

        // обновление будущей траектории
        int w = (trajLen / 2) / 10;
        for (int i = ((trajLen / 2) + 1); i < trajLen; i++) {
            float m = ((float)i - (float)(trajLen / 2) / 10.0f) % 1.0f;

            p[i].pos.x  = (1 - m) * Y[8 + (w * 0) + (i / 10) - w] + m * Y[8 + (w * 0) + (i / 10) - (w + 1)];
            p[i].pos.z  = (1 - m) * Y[8 + (w * 1) + (i / 10) - w] + m * Y[8 + (w * 1) + (i / 10) - (w + 1)];
            p[i].dir.x = (1 - m) * Y[8 + (w * 2) + (i / 10) - w] + m * Y[8 + (w * 2) + (i / 10) - (w + 1)];
            p[i].dir.z = (1 - m) * Y[8 + (w * 3) + (i / 10) - w] + m * Y[8 + (w * 3) + (i / 10) - (w + 1)];

            p[i].pos = (p[trajLen / 2].rot * p[i].pos) + p[trajLen / 2].pos;
            p[i].dir = Vector3.Normalize(p[trajLen / 2].rot * p[i].dir);
            p[i].rot = Quaternion.AngleAxis(
                (Mathf.Atan2(p[i].dir.x, p[i].dir.z) * Mathf.Rad2Deg),
                Vector3.up);
        }
    }

    public void UpPhase(Mtrx Y) {

        phase = (phase + ((ReturnStAmnt() * 0.9f) + 0.1f) * (2.0f * Mathf.PI) * Y[3, 0]) % (2.0f * Mathf.PI);
    }

    private float ReturnStAmnt() {

        return Mathf.Pow(1.0f - p[trajLen / 2].walkStand, 0.25f);
    }
    // показ траектории
    public void VisualTraject() {

        // Средняя значение
        for (int i = 0; i < trajLen; i += 10) {
            Vector3 posCenter = -p[i].pos;
            posCenter.Scale(new Vector3(scalFact, scalFact, scalFact));
            
            p[i].frPoint.transform.localPosition = -this.transform.position - posCenter;

            if ((i / 10) == 6) {
                this.transform.position = -posCenter;
            }
        }
        
        // Left and right point
        for (int i = 0; i < trajLen; i += 10) {
            // left
            Vector3 posLeft = Vector3.up + (p[i].rot * new Vector3(-sidePointsOffset * scalFact, 0.0f, 0.0f));
            p[i].frPoint.transform.GetChild(0).localPosition = posLeft;

            // right
            Vector3 posRight = Vector3.up + (p[i].rot * new Vector3(sidePointsOffset * scalFact, 0.0f, 0.0f));
            p[i].frPoint.transform.GetChild(2).localPosition = posRight;
        }

        // Direction arrow
        for (int i = 0; i < trajLen; i += 10) {
            Quaternion angle = Quaternion.AngleAxis(
                Mathf.Atan2(p[i].dir.x, p[i].dir.z) * Mathf.Rad2Deg,
                Vector3.up);//new Vector3(0.0f, 1.0f, 0.0f));
            angle *= new Quaternion(0.7f, 0.0f, 0.0f, 0.7f);
            p[i].frPoint.transform.GetChild(1).localRotation = angle;
        }
    }
    
    public void VisualBodyEl() {
        
        for (int i = 0; i < jNumber; i++) {
            Vector3 pos = bodies[i].pos;
            pos.Scale(new Vector3(scalFact, scalFact, scalFact));
     
            bodies[i].jPnt.transform.localPosition = new Vector3(
                this.transform.position.x - pos.x,
                -(this.transform.position.y - pos.y),
                this.transform.position.z - pos.z);
        }        
    }

    public void Crouch() {

        if (crTrgt == 0.0f) {
            crTrgt = 1.0f;
        } else {
            crTrgt = 0.0f;
        }
    }


}
