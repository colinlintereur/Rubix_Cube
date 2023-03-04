import javafx.application.Application;
import javafx.scene.Group;
import javafx.scene.Scene;
import javafx.scene.input.MouseEvent;
import javafx.stage.Stage;
import javafx.scene.paint.Color;
import javafx.scene.transform.Rotate;
import javafx.scene.transform.Transform;
import javafx.scene.transform.Translate;
import javafx.scene.PerspectiveCamera;
import javafx.scene.SceneAntialiasing;
import java.lang.Math;

public class Main extends Application {
  private double mousePosX, mousePosY, dx, dy = 0;
  private int rubixX, rubixY, rubixZ, cubeletSideLen, dim;
  private Group root;
  private PerspectiveCamera camera;
  private Rotate rotateX, rotateY;
  private Rubix rubix;
  private Scene scene;
  private Transform t, pivot, translate;
  
  private final double TURN_FACTOR = 1;

  @Override
  public void start(Stage stage) {
    // Set helper variables
    rubixX = 0;
    rubixY = 0;
    rubixZ = 0;
    cubeletSideLen = 50;
    dim = 3;

    t = new Rotate();
    pivot = new Translate(rubixX, rubixY, rubixZ);
    rotateX = new Rotate(0, Rotate.X_AXIS);
    rotateY = new Rotate(0, Rotate.Y_AXIS);

    // Set root group and add Rubix object
    root = new Group();
    rubix = new Rubix(rubixX, rubixY, rubixZ, cubeletSideLen, dim);
    root.getChildren().addAll(rubix);

    // Set scene and add in camera
    scene = new Scene(root, 1000, 1000, true, SceneAntialiasing.BALANCED);
    scene.setFill(Color.GREY);
    camera = new PerspectiveCamera(true);
    camera.setNearClip(0.1);
    camera.setFarClip(10000.0);
    translate = new Translate(0, 0,
        -1 * (scene.getHeight() / 2.) / Math.tan(Math.toRadians(camera.getFieldOfView()) / 2.));
    camera.getTransforms().addAll(pivot, t, translate);
    scene.setCamera(camera);
    handleMouseEvents();

    // Show stage
    stage.setTitle("My App");
    stage.setScene(scene);
    stage.show();
  }

  /**
   * This method is responsible for any mouse actions involving dragging the mouse pointer while a
   * mouse button is pressed.
   * While RMB is pressed:
   *   Rotate the camera around rubix so that it mimics the user rotating the cube itself.
   */
  private void handleMouseEvents() {
    scene.setOnMousePressed((MouseEvent me) -> {
      mousePosX = me.getSceneX();
      mousePosY = me.getSceneY();
    });

    scene.setOnMouseDragged((MouseEvent me) -> {
      dx = (mousePosX - me.getSceneX());
      dy = (mousePosY - me.getSceneY());

      if (me.isSecondaryButtonDown()) {
        rotateX = new Rotate((dy * 180 / scene.getHeight()) * TURN_FACTOR, Rotate.X_AXIS);
        t = t.createConcatenation(rotateX);

        rotateY = new Rotate((-dx * 180 / scene.getWidth()) * TURN_FACTOR, Rotate.Y_AXIS);
        t = t.createConcatenation(rotateY);

        camera.getTransforms().clear();
        camera.getTransforms().addAll(pivot, t, translate);

      }

      mousePosX = me.getSceneX();
      mousePosY = me.getSceneY();
    });
  }

  public static void main(String[] args) {
    launch();
  }

}
