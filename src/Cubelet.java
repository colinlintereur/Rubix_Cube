import javafx.scene.paint.Color;
import javafx.scene.paint.PhongMaterial;
import javafx.scene.Parent;
import javafx.scene.Node;
import javafx.collections.ObservableList;

public class Cubelet extends Parent {
  public int i, j, k;
  public Cubelet(int x, int y, int z, int i, int j, int k, int sideLen) {
    this.i = i;
    this.j = j;
    this.k = k;
    CBox box;

    //FRONT
    box = new CBox(x, y, z - (sideLen / 2), sideLen, sideLen, 0);
    if (k > 0) {
      box.setMaterial(new PhongMaterial(Color.BLACK));
      box.setSide(Side.INSIDE);
    } else {
      box.setMaterial(new PhongMaterial(Color.GREEN));
      box.setSide(Side.FRONT);
    }
    this.getChildren().add(box);

    //BACK
    box = new CBox(x, y, z + (sideLen / 2), sideLen, sideLen, 0);
    if (k < 2) {
      box.setMaterial(new PhongMaterial(Color.BLACK));
      box.setSide(Side.INSIDE);
    } else {
      box.setMaterial(new PhongMaterial(Color.BLUE));
      box.setSide(Side.BACK);
    }
    this.getChildren().add(box);

    //LEFT
    box = new CBox(x - (sideLen / 2), y, z, 0, sideLen, sideLen);
    if (i > 0) {
      box.setMaterial(new PhongMaterial(Color.BLACK));
      box.setSide(Side.INSIDE);
    } else {
      box.setMaterial(new PhongMaterial(Color.RED));
      box.setSide(Side.LEFT);
    }
    this.getChildren().add(box);

    //RIGHT
    box = new CBox(x + (sideLen / 2), y, z, 0, sideLen, sideLen);
    if (i < 2) {
      box.setMaterial(new PhongMaterial(Color.BLACK));
      box.setSide(Side.INSIDE);
    } else {
      box.setMaterial(new PhongMaterial(Color.ORANGE));
      box.setSide(Side.RIGHT);
    }
    this.getChildren().add(box);

    //UP
    box = new CBox(x, y - (sideLen / 2), z, sideLen, 0, sideLen);
    if (j > 0) {
      box.setMaterial(new PhongMaterial(Color.BLACK));
      box.setSide(Side.INSIDE);
    } else {
      box.setMaterial(new PhongMaterial(Color.YELLOW));
      box.setSide(Side.UP);
    }
    this.getChildren().add(box);

    //DOWN
    box = new CBox(x, y + (sideLen / 2), z, sideLen, 0, sideLen);
    if (j  < 2) {
      box.setMaterial(new PhongMaterial(Color.BLACK));
      box.setSide(Side.INSIDE);
    } else {
      box.setMaterial(new PhongMaterial(Color.WHITE));
      box.setSide(Side.DOWN);
    }
    this.getChildren().add(box);
  }

  @Override
  public ObservableList<Node> getChildren() {
    return super.getChildren();
  }
}
