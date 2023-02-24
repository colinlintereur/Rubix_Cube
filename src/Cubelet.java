import javafx.scene.shape.Box; 
import javafx.scene.paint.Color;
import javafx.scene.paint.PhongMaterial;
import javafx.scene.Parent;
import javafx.scene.Group;

public class Cubelet extends Parent{
  private PhongMaterial material;// = new PhongMaterial();

  public Cubelet(int x, int y, int z, int i, int j, int k, int sideLen) {
    Box box;
    // material.setDiffuseColor(color);

    //FRONT
    box = new Box(sideLen, sideLen, 0);
    if (k > 0) box.setMaterial(new PhongMaterial(Color.BLACK));
    else box.setMaterial(new PhongMaterial(Color.GREEN));
    box.setTranslateX(x);
    box.setTranslateY(y);
    box.setTranslateZ(z - (sideLen / 2));
    this.getChildren().add(box);

    //BACK
    box = new Box(sideLen, sideLen, 0);
    if (k < 2) box.setMaterial(new PhongMaterial(Color.BLACK));
    else box.setMaterial(new PhongMaterial(Color.BLUE));
    box.setTranslateX(x);
    box.setTranslateY(y);
    box.setTranslateZ(z + (sideLen / 2));
    this.getChildren().add(box);

    //LEFT
    box = new Box(0, sideLen, sideLen);
    if (i > 0) box.setMaterial(new PhongMaterial(Color.BLACK));
    else box.setMaterial(new PhongMaterial(Color.RED));
    box.setTranslateX(x - (sideLen / 2));
    box.setTranslateY(y);
    box.setTranslateZ(z);
    this.getChildren().add(box);

    //RIGHT
    box = new Box(0, sideLen, sideLen);
    if (i < 2) box.setMaterial(new PhongMaterial(Color.BLACK));
    else box.setMaterial(new PhongMaterial(Color.ORANGE));
    box.setTranslateX(x + (sideLen / 2));
    box.setTranslateY(y);
    box.setTranslateZ(z);
    this.getChildren().add(box);

    //UP
    box = new Box(sideLen, 0, sideLen);
    if (j > 0) box.setMaterial(new PhongMaterial(Color.BLACK));
    else box.setMaterial(new PhongMaterial(Color.YELLOW));
    box.setTranslateX(x);
    box.setTranslateY(y - (sideLen / 2));
    box.setTranslateZ(z);
    this.getChildren().add(box);

    //DOWN
    box = new Box(sideLen, 0, sideLen);
    if (j < 2) box.setMaterial(new PhongMaterial(Color.BLACK));
    else box.setMaterial(new PhongMaterial(Color.WHITE));
    box.setTranslateX(x);
    box.setTranslateY(y + (sideLen / 2));
    box.setTranslateZ(z);
    this.getChildren().add(box);

    // Box box = new Box(sideLen, sideLen, sideLen);
    // box.setMaterial(material);
    // box.setTranslateX(x);
    // box.setTranslateY(y);
    // box.setTranslateZ(z);
    // this.getChildren().add(box);
  }
}
