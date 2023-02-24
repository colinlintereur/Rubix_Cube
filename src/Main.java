import javafx.application.Application;
import javafx.scene.Group;
import javafx.scene.Node;
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
import javafx.scene.PerspectiveCamera;
import javafx.scene.SceneAntialiasing;
import java.lang.Math;

public class Main extends Application {
  private Scene scene;
  private Rotate rotateX;// = new Rotate(0, Rotate.X_AXIS);
  private Rotate rotateY;// = new Rotate(0, Rotate.Y_AXIS);
  private Rotate rotateZ;// = new Rotate(0, Rotate.Z_AXIS);
  private final double TURN_FACTOR = 1;
  Box box;
  private double mousePosX, mousePosY = 0;
  private double dx, dy = 0;
  private double totX, totY = 0;
  private double alf, bet, gam = 0;
  Rubix rubix;

  @Override
  public void start(Stage stage) {
    // Group root = new Group();
    // System.out.printf("%f, %f, %f", Rotate.Y_AXIS.getX(), Rotate.Y_AXIS.getY(),
    // Rotate.Y_AXIS.getZ());

    // final PhongMaterial redMaterial = new PhongMaterial();
    // redMaterial.setSpecularColor(Color.ORANGE);
    // redMaterial.setDiffuseColor(Color.RED);


    rubix = new Rubix(500, 500, 0, 50, 3);
    rotateX = new Rotate(0, 0, 500, 0, Rotate.X_AXIS);
    rotateY = new Rotate(0, 500, 0, 0, Rotate.Y_AXIS);
    rotateZ = new Rotate(0, 0, 0, 500, Rotate.Z_AXIS);
    rubix.getTransforms().addAll(rotateX, rotateY, rotateZ);
    // box = new Box(100, 100, 0);
    // rubix.setMaterial(redMaterial);
    // rubix.getTransforms().addAll(rotateX, rotateY, rotateZ);
    // rubix.setTranslateX(500);
    // rubix.setTranslateY(500);
    // rubix.setTranslateZ(0);



    // root.getChildren().add(rubix);
    scene = new Scene(rubix, 1000, 1000, true, SceneAntialiasing.BALANCED);
    scene.setFill(Color.GREY);
    // PerspectiveCamera camera = new PerspectiveCamera(true);
    // camera.setNearClip(0.1);
    // camera.setFarClip(10000.0);
    // camera.setTranslateZ(-10);
    // scene.setCamera(camera);
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
      dx = (mousePosX - me.getSceneX());
      dy = (mousePosY - me.getSceneY());
      totX += dx;
      totY += dy;
      if (me.isSecondaryButtonDown()) {
        // rotateX.setAngle(
        //     rotateX.getAngle() - (dy / rubix.getLen() * 360) * (Math.PI / 180) * TURN_FACTOR);
        // rotateY.setAngle(
        //     rotateY.getAngle() - (dx / rubix.getLen() * -360) * (Math.PI / 180) * TURN_FACTOR);
        alf = 0;
        gam = Math.IEEEremainder(-totX / rubix.getLen() * TURN_FACTOR, 2*Math.PI);
        bet = Math.abs((Math.PI/2) - Math.abs(gam)) * (2/Math.PI) * Math.IEEEremainder((totY / rubix.getLen() * TURN_FACTOR), 2*Math.PI);
        matrixRotateNode(alf, bet, gam);
      }
      // System.out.printf("angleX: %f, angleY: %f\n", rotateX.getAngle(), rotateY.getAngle());
      mousePosX = me.getSceneX();
      mousePosY = me.getSceneY();
    });
  }

  private void matrixRotateNode(double alf, double bet, double gam) {
    System.out.printf("%f %f %f\n", alf, bet, gam);
    double A11=Math.cos(alf)*Math.cos(gam);
    double A12=Math.cos(bet)*Math.sin(alf)+Math.cos(alf)*Math.sin(bet)*Math.sin(gam);
    double A13=Math.sin(alf)*Math.sin(bet)-Math.cos(alf)*Math.cos(bet)*Math.sin(gam);
    double A21=-Math.cos(gam)*Math.sin(alf);
    double A22=Math.cos(alf)*Math.cos(bet)-Math.sin(alf)*Math.sin(bet)*Math.sin(gam);
    double A23=Math.cos(alf)*Math.sin(bet)+Math.cos(bet)*Math.sin(alf)*Math.sin(gam);
    double A31=Math.sin(gam);
    double A32=-Math.cos(gam)*Math.sin(bet);
    double A33=Math.cos(bet)*Math.cos(gam);
    
    double d = Math.acos((A11+A22+A33-1d)/2d);
    if(d!=0d){
        double den=2d*Math.sin(d);
        Point3D p= new Point3D((A32-A23)/den,(A13-A31)/den,(A21-A12)/den);
        rubix.setRotationAxis(p);
        rubix.setRotate(Math.toDegrees(d));
    }
}

  public static void main(String[] args) {
    launch();
  }

}
