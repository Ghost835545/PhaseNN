using System;
using System.Collections;
using System.Collections.Generic;

public class Mtrx {

    private float[] records;
    private int r, clmn;

    public Mtrx(int r, int clmn) {

        this.r = r;
        this.clmn = clmn;
        records = new float[r * clmn];
    }

    /// <summary>
    /// Creates vertical one-dimensional matrix.
    /// </summary>
    /// <param name="rows"></param>
    public Mtrx(int rows) : this(rows, 1) {}

    public Mtrx(float[,] records) : this(records.GetLength(0), records.GetLength(1)) {

        int index = 0;
        for (int row = 0; row < r; row++) {
            for (int column = 0; column < clmn; column++) {
                this.records[index] = records[row, column];
                ++index;
            }
        }
    }

    public Mtrx(Mtrx mat) : this(mat.r, mat.clmn) {

        for (int i = 0; i < records.Length; i++) {
            records[i] = mat.records[i];
        }
    }

    public float this[int r, int cl] {
        get {
            return records[(r * this.clmn) + cl];
        }
        set {
            records[(r * this.clmn) + cl] = value;
        }
    }


    // Возвращает первый элемент в выбранной строке. Работает только для вертикальных одномерных матриц!!!!
    public float this[int r] {
        get {
            return records[(r * this.clmn) + 0];
        }
        set {
            records[(r * this.clmn) + 0] = value;
        }
    }
    
    public static Mtrx operator + (Mtrx mat1, Mtrx mat2) {

        if (mat1.SameSize(mat2)) {
            Mtrx result = new Mtrx(mat1.r, mat2.clmn);

            for (int i = 0; i < mat1.records.Length; i++) {
                result.records[i] = mat1.records[i] + mat2.records[i];
            }
            return result;

        } else {
            throw new InvalidMatrixException("Matrix dimensions do not match.");
        }
    }

    public static Mtrx operator - (Mtrx mat1, Mtrx mat2) {

        if (mat1.SameSize(mat2)) {
            Mtrx result = new Mtrx(mat1.r, mat2.clmn);

            for (int i = 0; i < mat1.records.Length; i++) {
                result.records[i] = mat1.records[i] - mat2.records[i];
            }
            return result;

        } else {
            throw new InvalidMatrixException("Matrix dimensions do not match.");
        }
    }

    public bool SameSize(Mtrx mat) {

        if ((this.r == mat.r) && (this.clmn == mat.clmn)) {
            return true;
        } else {
            return false;
        }
    }

    public static Mtrx operator * (Mtrx mat1, Mtrx mat2) {

        if (mat1.MtrxSameSizeAndVert(mat2)) {                
            Mtrx result = new Mtrx(mat1.r, 1);

            for (int i = 0; i < mat1.r; i++) {
                result[i, 0] = mat1[i, 0] * mat2[i, 0];
            }
            return result;

        } else if (mat1.IsGenerationMtrx(mat2)) {
            Mtrx result = new Mtrx(mat1.r, mat2.clmn);

            for (int i = 0; i < mat1.r; i++) {
                MultiplyRow(i, mat1, mat2, ref result);
            }
            return result;

        } else {
            throw new InvalidMatrixException("Error, Multiplying is not possible. First matrix column size is not same as second matrix rows.");
        }
    }

    public bool MtrxSameSizeAndVert(Mtrx mat) {

        if (this.r == mat.r && this.clmn == 1 && mat.clmn == 1) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsGenerationMtrx(Mtrx mat) {

        if (this.clmn == mat.r) {
            return true;
        } else {
            return false;
        }
    }

    public static void MultiplyRow(int row, Mtrx mat1, Mtrx mat2, ref Mtrx resultMat) {

        int mat1Index = row * mat1.clmn;
        int mat2Index;

        for (int column = 0; column < resultMat.clmn; column++) {
            float result = 0;
            mat2Index = column;

            for (int i = 0; i < mat1.clmn; i++) {
                result += mat1.records[mat1Index + i] * mat2.records[mat2Index];
                mat2Index += mat2.clmn;
            }

            resultMat[row, column] = result;
        }
    }

    /*
     * Используется только для вертикальных матриц одинкавого размера (Nx1), чтобы mat1/mat2
     */
    public static Mtrx operator / (Mtrx mat1, Mtrx mat2) {

        if (mat1.IsMtrxVert() && mat2.IsMtrxVert()) {
            Mtrx result = new Mtrx(mat1.r, 1);

            for (int i = 0; i < mat1.r; i++) {
                result[i, 0] = mat1[i, 0] / mat2[i, 0];
            }
            return result;

        } else {
            throw new InvalidMatrixException(
                "All matrices must be vertical. " +
                "Error, Multiplying is not possible. First matrix column size is not same as second matrix rows."
                );
        }
    }

    private bool IsMtrxVert() {

        if (this.clmn == 1) {
            return true;
        } else {
            return false;
        }
    }

    
    //Экспоненциальная линейная функция. (активационная функция)
    public void ELU() {

        for (int i = 0; i < this.records.Length; i++) {
            records[i] = (float)(Math.Max(records[i], 0) + Math.Exp(Math.Min(records[i], 0)) - 1);
        }
    }

}

public class InvalidMatrixException : InvalidOperationException {
    public InvalidMatrixException() {}

    public InvalidMatrixException(string message)
        : base(message) {
    }

    public InvalidMatrixException(string message, Exception inner)
        : base(message, inner) {
    }
}
