import javafx.scene.transform.Rotate;
import javafx.geometry.Point3D;

public enum Side {
  FRONT(Rotate.Z_AXIS),
  BACK(Rotate.Z_AXIS),
  LEFT(Rotate.X_AXIS),
  RIGHT(Rotate.X_AXIS),
  UP(Rotate.Y_AXIS),
  DOWN(Rotate.Y_AXIS),
  INSIDE(null);

  private Point3D value;

  private Side(Point3D value) {
    this.value = value;
  }

  public Point3D getValue() {
    return this.value;
  }
}