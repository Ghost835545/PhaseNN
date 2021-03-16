using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PFNN_COMPUTE {

    private const float PI = 3.14159274f;
    private string WeightFolderPath = "D:\\Net\\";
    
    private int InSize;
    private int OutSize;
    private int CountOfNeuron;

    // inputs, outputs
    public Mtrx X, Y;
    // hidden layers
    private Mtrx H0, H1;                      

    private Mtrx Xmean, Xstd, Ymean, Ystd;
    // weights
    private Mtrx[] W0, W1, W2;
    // biases
    private Mtrx[] B0, B1, B2;                

    public enum Mode {
        constant,
        line,
        cub
    }
    private Mode WghtMode;
    public PFNN_COMPUTE (
        Mode weightsType = Mode.constant,
        int inputSize = 342, 
        int outputSize = 311, 
        int numberOfNeurons = 512
        ) {

        WghtMode = weightsType;
        InSize = inputSize;
        OutSize = outputSize;
        CountOfNeuron = numberOfNeurons;
        
        SetWCount();
        SLayerSize();

        LMeanAndStds();
        LWeig();
    }

    private void SetWCount() {

        switch (WghtMode) {
            case Mode.constant:
                W0 = new Mtrx[50]; W1 = new Mtrx[50]; W2 = new Mtrx[50];
                B0 = new Mtrx[50]; B1 = new Mtrx[50]; B2 = new Mtrx[50];
                break;
            case Mode.line:
                W0 = new Mtrx[10]; W1 = new Mtrx[10]; W2 = new Mtrx[10];
                B0 = new Mtrx[10]; B1 = new Mtrx[10]; B2 = new Mtrx[10];
                break;
            case Mode.cub:
                W0 = new Mtrx[4]; W1 = new Mtrx[4]; W2 = new Mtrx[4];
                B0 = new Mtrx[4]; B1 = new Mtrx[4]; B2 = new Mtrx[4];
                break;
        }
    }

    public void LMeanAndStds() {

        GetDataFromFile(out Xmean, InSize, "Xmean.bin");
        GetDataFromFile(out Xstd, InSize, "Xstd.bin");
        GetDataFromFile(out Ymean, OutSize, "Ymean.bin");
        GetDataFromFile(out Ystd, OutSize, "Ystd.bin");
    }

    public void LWeig() {        

        int j;
        switch (WghtMode) {            
            case Mode.constant:
                for (int i = 0; i < 50; i++) {
                    
                    GetDataFromFile(out W0[i], CountOfNeuron, InSize, string.Format("W0_{0:000}.bin", i));
                    GetDataFromFile(out W1[i], CountOfNeuron, CountOfNeuron, string.Format("W1_{0:000}.bin", i));
                    GetDataFromFile(out W2[i], OutSize, CountOfNeuron, string.Format("W2_{0:000}.bin", i));

                    GetDataFromFile(out B0[i], CountOfNeuron, string.Format("b0_{0:000}.bin", i));
                    GetDataFromFile(out B1[i], CountOfNeuron, string.Format("b1_{0:000}.bin", i));
                    GetDataFromFile(out B2[i], OutSize, string.Format("b2_{0:000}.bin", i));
                }
                break;
            case Mode.line:                
                for (int i = 0; i < 10; i++) {
                    j = i * 5;

                    GetDataFromFile(out W0[i], CountOfNeuron, InSize, string.Format("W0_{0:000}.bin", j));
                    GetDataFromFile(out W1[i], CountOfNeuron, CountOfNeuron, string.Format("W1_{0:000}.bin", j));
                    GetDataFromFile(out W2[i], OutSize, CountOfNeuron, string.Format("W2_{0:000}.bin", j));

                    GetDataFromFile(out B0[i], CountOfNeuron, string.Format("b0_{0:000}.bin", j));
                    GetDataFromFile(out B1[i], CountOfNeuron, string.Format("b1_{0:000}.bin", j));
                    GetDataFromFile(out B2[i], OutSize, string.Format("b2_{0:000}.bin", j));
                }
                break;
            case Mode.cub:
                for (int i = 0; i < 4; i++) {
                    j = (int)(i * 12.5);

                    GetDataFromFile(out W0[i], CountOfNeuron, InSize, string.Format("W0_{0:000}.bin", j));
                    GetDataFromFile(out W1[i], CountOfNeuron, CountOfNeuron, string.Format("W1_{0:000}.bin", j));
                    GetDataFromFile(out W2[i], OutSize, CountOfNeuron, string.Format("W2_{0:000}.bin", j));

                    GetDataFromFile(out B0[i], CountOfNeuron, string.Format("b0_{0:000}.bin", j));
                    GetDataFromFile(out B1[i], CountOfNeuron, string.Format("b1_{0:000}.bin", j));
                    GetDataFromFile(out B2[i], OutSize, string.Format("b2_{0:000}.bin", j));
                }
                break;
        }
    }

    private void GetDataFromFile(out Mtrx itm, int r, string fName) {

        itm = new Mtrx(r);

        string fullPath = WeightFolderPath + fName;
        float value;
        if (File.Exists(fullPath)) {
            using (BinaryReader reader = new BinaryReader(File.Open(fullPath, FileMode.Open))) {

                for (int i = 0; i < r; i++) {
                    value = reader.ReadSingle();
                    itm[i] = value;
                }
            }
        }
    }

    private void GetDataFromFile(out Mtrx itm, int r, int clmn, string fName) {

        itm = new Mtrx(r, clmn);

        string fullPath = WeightFolderPath + fName;
        float value;
        if (File.Exists(fullPath)) {
            using (BinaryReader reader = new BinaryReader(File.Open(fullPath, FileMode.Open))) {

                for (int i = 0; i < r; i++) {
                    for (int j = 0; j < clmn; j++) {
                        value = reader.ReadSingle();
                        itm[i, j] = value;
                    }
                }
            }
        }
    }

    private void SLayerSize() {

        X = new Mtrx(InSize);
        Y = new Mtrx(OutSize);

        H0 = new Mtrx(CountOfNeuron);
        H1 = new Mtrx(CountOfNeuron);
    }

    /// <summary>
    /// Main function for computing Neural Network result.
    /// </summary>
    /// <param name="p">Phase value.</param>
    public void Compute(float p) {

        int pIndex0;

        X = (X - Xmean) / Xstd;

        switch (WghtMode) {
            case Mode.constant:
                pIndex0 = (int)((p / (2 * PI)) * 50);

                // Layer 1
                H0 = (W0[pIndex0] * X) + B0[pIndex0];
                H0.ELU();

                // Layer 2
                H1 = (W1[pIndex0] * H0) + B1[pIndex0];
                H1.ELU();

                // Layer 3, network output
                Y = (W2[pIndex0] * H1) + B2[pIndex0];
                break;

            case Mode.line:
                break;

            case Mode.cub:
                break;
        }

        Y = (Y * Ystd) + Ymean;
    }

    public void Reset() {

        Y = Ymean;
    }


}
