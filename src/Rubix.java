import javafx.scene.Parent;
import javafx.scene.shape.Line;
import javafx.scene.transform.Rotate;
import javafx.scene.paint.Color;
import javafx.scene.text.Text;

public class Rubix extends Parent {
  private int cubeletSideLen;
  public Cubelet[][][] cubelets;

  private int GAP = 2;

  public Rubix(int x, int y, int z, int cubeletSideLen, int dim) {
    this.cubeletSideLen = cubeletSideLen;
    this.cubelets = new Cubelet[dim][dim][dim];

    // RUBIX CUBE
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

    // AXES
    Line xAxis = new Line(x, y, x + (offset * 3), y);
    xAxis.setStroke(Color.RED);
    xAxis.setStrokeWidth(5);
    Text xString = new Text(x + (offset * 3) + 20, y + 5, "X");

    Line yAxis = new Line(x, y, x, y + (offset * 3));
    yAxis.setStroke(Color.GREEN);
    yAxis.setStrokeWidth(5);
    Text yString = new Text(x - 5, y + (offset * 3) + 20, "Y");

    Line zAxis = new Line(x, y, x, y + (offset * 3));
    zAxis.setStroke(Color.BLUE);
    zAxis.setStrokeWidth(5);
    Text zString = new Text(x - 5, y + (offset * 3) + 20, "Z");

    Rotate rotate = new Rotate(90, x, y, z, Rotate.X_AXIS);
    zAxis.getTransforms().add(rotate);
    zString.getTransforms().add(rotate);
    this.getChildren().addAll(xAxis, yAxis, zAxis, xString, yString, zString);
  }

  public int getLen() {
    return (2 * GAP) + (3 * this.cubeletSideLen);
  }
}
