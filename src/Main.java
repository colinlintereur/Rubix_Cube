import javafx.application.Application;
import javafx.scene.Group;
import javafx.scene.Scene;
import javafx.scene.control.Label;
import javafx.scene.input.MouseEvent;
import javafx.scene.layout.StackPane;
import javafx.stage.Stage;
import javafx.scene.shape.Box;
import javafx.scene.paint.Color;
import javafx.scene.paint.PhongMaterial;
import javafx.scene.transform.Rotate;
import javafx.event.EventHandler;
import javafx.geometry.Point3D;

public class Main extends Application {
  private Scene scene;
  private Rotate rotateX;// = new Rotate(0, Rotate.X_AXIS);
  private Rotate rotateY;// = new Rotate(0, Rotate.Y_AXIS);
  private Rotate rotateZ;// = new Rotate(0, Rotate.Z_AXIS);
  private final double TURN_FACTOR = 3;
  Box box;
  private double mousePosX, mousePosY = 0;
  Rubix rubix;

  @Override
  public void start(Stage stage) {
    Group root = new Group();
    // System.out.printf("%f, %f, %f", Rotate.Y_AXIS.getX(), Rotate.Y_AXIS.getY(),
    // Rotate.Y_AXIS.getZ());

    // final PhongMaterial redMaterial = new PhongMaterial();
    // redMaterial.setSpecularColor(Color.ORANGE);
    // redMaterial.setDiffuseColor(Color.RED);


    rubix = new Rubix(500, 500, 0, 50, 3);
    // rotateX = new Rotate(0, new Point3D(500, 0, 0));
    // rotateY = new Rotate(0, new Point3D(0, 500, 0));
    rotateX = new Rotate(0, 0, 500, 0, Rotate.X_AXIS);
    rotateY = new Rotate(0, 500, 0, 0, Rotate.Y_AXIS);
    rotateZ = new Rotate(0, Rotate.Z_AXIS);
    rubix.getTransforms().addAll(rotateX, rotateY, rotateZ);
    // box = new Box(100, 100, 0);
    // box.setMaterial(redMaterial);
    // box.getTransforms().addAll(rotateX, rotateY, rotateZ);
    // box.setTranslateX(500);
    // box.setTranslateY(500);
    // box.setTranslateZ(200);



    root.getChildren().add(rubix);
    scene = new Scene(root, 1000, 1000);
    handleMouseEvents();
    // scene.addEventFilter(MouseEvent.MOUSE_CLICKED, new EventHandler<MouseEvent>() {
    // @Override
    // public void handle(MouseEvent mouseEvent) {
    // if (mouseEvent.isSecondaryButtonDown() && mouseEvent.isDragDetect()) {

    // }
    // }
    // });


    stage.setTitle("My App");
    stage.setScene(scene);
    stage.show();
  }

  private void handleMouseEvents() {
    scene.setOnMousePressed((MouseEvent me) -> {
      mousePosX = me.getSceneX();
      mousePosY = me.getSceneY();
    });

    scene.setOnMouseDragged((MouseEvent me) -> {
      double dx = (mousePosX - me.getSceneX());
      double dy = (mousePosY - me.getSceneY());
      if (me.isSecondaryButtonDown()) {
        rotateX.setAngle(
            rotateX.getAngle() - (dy / rubix.getLen() * 360) * (Math.PI / 180) * TURN_FACTOR);
        rotateY.setAngle(
            rotateY.getAngle() - (dx / rubix.getLen() * -360) * (Math.PI / 180) * TURN_FACTOR);
      }
      mousePosX = me.getSceneX();
      mousePosY = me.getSceneY();
    });
  }

  public static void main(String[] args) {
    launch();
  }

}
