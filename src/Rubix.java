import javafx.scene.Parent;

public class Rubix extends Parent {
  private int x;
  private int y;
  private int z;
  private int cubeletSideLen;
  public int dim;
  public Cubelet[][][] cubelets;

  private int GAP = 2;

  public Rubix(int x, int y, int z, int cubeletSideLen, int dim) {
    this.x = x;
    this.y = y;
    this.z = z;
    this.cubeletSideLen = cubeletSideLen;
    this.dim = dim;
    this.cubelets = new Cubelet[dim][dim][dim];

    int offset = cubeletSideLen + GAP;
    for (int i = 0; i < dim; i++) {
      for (int j = 0; j < dim; j++) {
        for (int k = 0; k < dim; k++) {
          this.cubelets[i][j][k] = new Cubelet(x + (offset * (i - 1)), y + (offset * (j - 1)),
              z + (offset * (k - 1)), i, j, k, cubeletSideLen);
          this.getChildren().add(this.cubelets[i][j][k]);
        }
      }
    }
  }

  public int getLen() {
    return (2 * GAP) + (3 * this.cubeletSideLen);
  }
}
