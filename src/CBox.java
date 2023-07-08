import javafx.scene.shape.Box;

public class CBox extends Box {
  private Side side;
  public CBox(int x, int y, int z, int width, int height, int depth) {
    super(width, height, depth);
    this.setTranslateX(x);
    this.setTranslateY(y);
    this.setTranslateZ(z);
  }

  protected void setSide(Side s) {
    this.side = s;
    return;
  }

  public Side getSide() {
    return this.side;
  }
}