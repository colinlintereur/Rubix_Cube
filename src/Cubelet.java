import javafx.scene.shape.Box; 
import javafx.scene.paint.Color;
import javafx.scene.paint.PhongMaterial;
import javafx.scene.Parent;

public class Cubelet extends Parent{
  private int x;
  private int y;
  private int z;
  private int sideLen;

  final PhongMaterial redMaterial = new PhongMaterial();

  enum Colors {
    RED,
    ORANGE,
    YELLOW,
    GREEN,
    BLUE,
    WHITE,
    BLACK
  }

  public Cubelet(int x, int y, int z, int sideLen) {
    this.x = x;
    this.y = y;
    this.z = z;
    this.sideLen = sideLen;

    redMaterial.setSpecularColor(Color.ORANGE);
    redMaterial.setDiffuseColor(Color.RED);

    Box box = new Box(sideLen, sideLen, 0);
    box.setMaterial(redMaterial);
    box.setTranslateX(x);
    box.setTranslateY(y);
    box.setTranslateZ(z);
    this.getChildren().add(box);
    // box.getTransforms().addAll(rotateX, rotateY, rotateZ);
  }
}
