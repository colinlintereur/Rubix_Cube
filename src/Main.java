import javafx.application.Application;
import javafx.scene.Group;
import javafx.scene.Scene;
import javafx.scene.input.MouseEvent;
import javafx.stage.Stage;
import javafx.scene.paint.Color;
import javafx.scene.transform.Rotate;
import javafx.scene.transform.Transform;
import javafx.scene.transform.Translate;
import javafx.scene.shape.Box; 
import javafx.scene.SceneAntialiasing;
import java.lang.Math;
import javafx.scene.transform.NonInvertibleTransformException;
import javafx.scene.Node;
import javafx.geometry.Point3D;
import javafx.geometry.Point2D;
import javafx.util.Pair;
import javafx.event.EventHandler;
import java.util.*;

public class Main extends Application {
  private double mousePosX, mousePosY, dx, dy, totX, totY = 0;
  private int rubixX, rubixY, rubixZ, cubeletSideLen, dim;
  private Group root;//, groupX, groupY, groupZ;
  private Rotate rotateX, rotateY;
  private Rubix rubix;
  private Scene scene;
  private Transform t, pivot, t2, tInv, tRotates;
  Box box;
  private Rotate rotateBoxX, rotateBoxY, rotateBoxZ, rotateGroup;
  private Point3D axis, nonAxis;
  Map<Point3D, ArrayList<Cubelet>> groups;
  Map<Point3D, ArrayList<Side>> sides;
  private double dotsKey;
  private Map<Double, Pair<Point3D, Point2D>> dots;
  private EventHandler<MouseEvent> handler;
  Group rotatingGroup;

  private final double TURN_FACTOR = 1;

  @Override
  public void start(Stage stage) {
    // Set helper variables
    rubixX = 500;
    rubixY = 500;
    rubixZ = 0;
    cubeletSideLen = 50;
    dim = 3;

    t = new Rotate();
    t2 = new Rotate();
    tInv = new Rotate();
    tRotates = new Rotate();
    pivot = new Translate(rubixX, rubixY, rubixZ);
    rotateX = new Rotate(0, Rotate.X_AXIS);
    rotateY = new Rotate(0, Rotate.Y_AXIS);
    rotateBoxX = new Rotate(0, Rotate.X_AXIS);
    rotateBoxY = new Rotate(0, Rotate.Y_AXIS);
    rotateBoxZ = new Rotate(0, Rotate.Z_AXIS);
    rotateGroup = new Rotate();
    // groupX = new Group();
    // groupY = new Group();
    // groupZ = new Group();
    groups = new HashMap<>();
    // groups.put(Rotate.X_AXIS, new Group());
    // groups.put(Rotate.Y_AXIS, new Group());
    // groups.put(Rotate.Z_AXIS, new Group());
    groups.put(Rotate.X_AXIS, new ArrayList<>());
    groups.put(Rotate.Y_AXIS, new ArrayList<>());
    groups.put(Rotate.Z_AXIS, new ArrayList<>());

    sides = new HashMap<>();
    sides.put(Rotate.X_AXIS, new ArrayList<>(List.of(Side.FRONT, Side.DOWN, Side.BACK, Side.UP)));
    sides.put(Rotate.Y_AXIS, new ArrayList<>(List.of(Side.FRONT, Side.LEFT, Side.BACK, Side.RIGHT)));
    sides.put(Rotate.Z_AXIS, new ArrayList<>(List.of(Side.RIGHT, Side.DOWN, Side.LEFT, Side.UP)));
    axis = null;
    nonAxis = null;
    rotatingGroup = new Group();
    handler = MouseEvent::consume;

    // Set root group and add Rubix object
    root = new Group();
    rubix = new Rubix(cubeletSideLen, dim);
    // rubix.getTransforms().addAll(pivot, t);
    for (Node node : rubix.getChildren()) {
      // node.getTransforms().addAll(pivot, tInv);
      node.getTransforms().addAll(pivot, rotateBoxZ, rotateBoxY, rotateBoxX, tRotates);
    }
    // box = new Box(200, 100, 50);
    // box.getTransforms().addAll(pivot, t2);// rotateBoxX, rotateBoxY, rotateBoxZ);
    root.getChildren().addAll(rubix, rotatingGroup);

    // Set scene
    scene = new Scene(root, 1000, 1000, true, SceneAntialiasing.BALANCED);
    scene.setFill(Color.GREY);
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
    for (Node node : rubix.getChildren()) {
      if (node instanceof Cubelet) {
        for (Node node2 : ((Cubelet)node).getChildren()) {
          if (node2 instanceof CBox) {
            // System.out.printf("setting events: %s\n", ((CBox)node2).getSide());
            node2.setOnMousePressed((MouseEvent me) -> {
              // System.out.printf("node clicked: %s\n", ((CBox)node2).getSide());
              if (me.isPrimaryButtonDown()) {
                Side side = ((CBox)node2).getSide();
                System.out.printf("node clicked: %s\n", side);
                // List<Cubelet> listX = new ArrayList<>();
                // List<Cubelet> listY = new ArrayList<>();
                // List<Cubelet> listZ = new ArrayList<>();
                switch (side) {
                  case FRONT:
                  case BACK:
                    for (int j = 0; j <= 2; j++) {
                      for (int k = 0; k <= 2; k++) {
                        // groups.get(Rotate.X_AXIS).getChildren().add(rubix.cubelets[((Cubelet)node).i][j][k]);
                        groups.get(Rotate.X_AXIS).add(rubix.cubelets[((Cubelet)node).i][j][k]);
                      }
                    }
                    for (int i = 0; i <= 2; i++) {
                      for (int k = 0; k <= 2; k++) {
                        // groups.get(Rotate.Y_AXIS).getChildren().add(rubix.cubelets[i][((Cubelet)node).j][k]);
                        groups.get(Rotate.Y_AXIS).add(rubix.cubelets[i][((Cubelet)node).j][k]);
                      }
                    }
                    // System.out.println("added cubelets to groups");
                    nonAxis = Rotate.Z_AXIS;
                    break;
                  // case LEFT:
                  // case RIGHT:
                  //   for (int i = 0; i < 2; i++) {
                  //     for (int k = 0; k < 2; k++) {
                  //       groups.get(Rotate.Y_AXIS).getChildren().add(rubix.cubelets[i][((Cubelet)node).j][k]);
                  //     }
                  //   }
                  //   for (int i = 0; i < 2; i++) {
                  //     for (int j = 0; j < 2; j++) {
                  //       groups.get(Rotate.Z_AXIS).getChildren().add(rubix.cubelets[i][j][((Cubelet)node).k]);
                  //     }
                  //   }
                  //   nonAxis = Rotate.X_AXIS;
                  //   break;
                  // case UP:
                  // case DOWN:
                  //   for (int j = 0; j < 2; j++) {
                  //     for (int k = 0; k < 2; k++) {
                  //       groups.get(Rotate.X_AXIS).getChildren().add(rubix.cubelets[((Cubelet)node).i][j][k]);
                  //     }
                  //   }
                  //   for (int i = 0; i < 2; i++) {
                  //     for (int j = 0; j < 2; j++) {
                  //       groups.get(Rotate.Z_AXIS).getChildren().add(rubix.cubelets[i][j][((Cubelet)node).k]);
                  //     }
                  //   }
                  //   nonAxis = Rotate.Y_AXIS;
                  //   break;
                  case INSIDE:
                  default:
                    break;
                }
              }
            });
          }
        }
      }
    }

    scene.setOnMousePressed((MouseEvent me) -> {
      scene.addEventFilter(MouseEvent.MOUSE_CLICKED, handler);
      
      mousePosX = me.getSceneX();
      mousePosY = me.getSceneY();

      if (me.isPrimaryButtonDown()) {
        totX = 0;
        totY = 0;
        // Point3D pX3 = t2.transform(Rotate.X_AXIS);
        // Point3D pY3 = t2.transform(Rotate.Y_AXIS);
        // Point3D pZ3 = t2.transform(Rotate.Z_AXIS);

        // Point2D pX2Perp = new Point2D(Rotate.Y_AXIS.dotProduct(pX3), -1 * Rotate.X_AXIS.dotProduct(pX3));
        // Point2D pY2Perp = new Point2D(Rotate.Y_AXIS.dotProduct(pY3), -1 * Rotate.X_AXIS.dotProduct(pY3));
        // Point2D pZ2Perp = new Point2D(Rotate.Y_AXIS.dotProduct(pZ3), -1 * Rotate.X_AXIS.dotProduct(pZ3));

        // // double pX2_slope = pX2Perp.getY() / pX2Perp.getX();
        // // double pY2_slope = pY2Perp.getY() / pY2Perp.getX();
        // // double pZ2_slope = pZ2Perp.getY() / pZ2Perp.getX();

        // Point2D mouse = new Point2D(dx, dy);
        // double xDot = pX2Perp.dotProduct(mouse);
        // double yDot = pY2Perp.dotProduct(mouse);
        // double zDot = pZ2Perp.dotProduct(mouse);

        // // if ((xDot >= yDot) && (xDot >= zDot) && (nonAxis != Rotate.X_AXIS)) {
        // //   axis = Rotate.X_AXIS;
        // // } else if ((yDot >= xDot) && (yDot >= zDot) && (nonAxis != Rotate.Y_AXIS)) {
        // //   axis = Rotate.Y_AXIS;
        // // } else if ((zDot >= xDot) && (zDot >= yDot) && (nonAxis != Rotate.Z_AXIS)) {
        // //   axis = Rotate.Z_AXIS;
        // // } else {
        // //   System.out.println("SOMETHING HAS GONE HORRIBLY WRONG: rotation is close to no axis");
        // // }

        // dots = new HashMap<>();
        // dots.put(xDot, new Pair<>(Rotate.X_AXIS, pX2Perp));
        // dots.put(yDot, new Pair<>(Rotate.Y_AXIS, pY2Perp));
        // dots.put(zDot, new Pair<>(Rotate.Z_AXIS, pZ2Perp));
        // TreeMap<Double, Pair<Point3D, Point2D>> sorted = new TreeMap<>();
        // sorted.putAll(dots);

        // for (Map.Entry<Double, Pair<Point3D, Point2D>> entry : sorted.entrySet()) {
        //   if (entry.getValue().getKey() == nonAxis) continue;
        //   axis = entry.getValue().getKey();
        //   dotsKey = entry.getKey();
        //   break;
        // }

        
        // rotateGroup = new Rotate();

        // rotatingGroup.getChildren().addAll(groups.get(axis));
        // rotatingGroup.getTransforms().addAll(pivot, tInv, rotateGroup);

      }
    });

    scene.setOnMouseReleased((MouseEvent me) -> {
      if (nonAxis != null) {
        // rubix.getChildren().addAll(rotatingGroup.getChildren());
        // rotatingGroup.getChildren().clear();
        // rotatingGroup.getTransforms().clear();
        // System.out.println("cleared rotatingGroup");
        double angle = rotateGroup.getAngle();
        angle = angle - Math.IEEEremainder(angle, 90.);
        rotateGroup.setAngle(angle);
        // System.out.printf("angle: %f, mod: %f\n", angle, (Math.abs(angle) % 90.));
        if ((int)angle != 0) {
          System.out.println("updating sides");
          updateSides(groups, axis, angle);
        }
        if (axis != null) {
          for (Cubelet cubelet : groups.get(axis)) {
            cubelet.getTransforms().remove(cubelet.getTransforms().size() - 1);
            tRotates = cubelet.getTransforms().remove(cubelet.getTransforms().size() - 1);
            tRotates = tRotates.createConcatenation(rotateGroup);
            cubelet.getTransforms().add(tRotates);
          }
        }
        axis = null;
        nonAxis = null;
      }
      
      groups.put(Rotate.X_AXIS, new ArrayList<>());
      groups.put(Rotate.Y_AXIS, new ArrayList<>());
      groups.put(Rotate.Z_AXIS, new ArrayList<>());
      scene.removeEventFilter(MouseEvent.MOUSE_CLICKED, handler);
    });

    scene.setOnMouseDragged((MouseEvent me) -> {
      dx = (mousePosX - me.getSceneX());
      dy = (mousePosY - me.getSceneY());

      if (me.isSecondaryButtonDown()) {
        // tInv = t;
        rotateX = new Rotate((dy * 180 / scene.getHeight()) * TURN_FACTOR, Rotate.X_AXIS);
        t = t.createConcatenation(rotateX);

        rotateY = new Rotate((-dx * 180 / scene.getWidth()) * TURN_FACTOR, Rotate.Y_AXIS);
        t = t.createConcatenation(rotateY);

        try {
          tInv = t.createInverse();
        }
        catch (NonInvertibleTransformException e) {
          System.out.println(e);
        }

        // rubix.getTransforms().clear();
        // rubix.getTransforms().addAll(pivot, tInv);
        // for (Node node : rubix.getChildren()) {
        //   node.getTransforms().clear();
        //   node.getTransforms().addAll(pivot, tInv);
        // }


        double thetaX, thetaY, thetaZ;
        thetaX = Math.atan2(tInv.getMzy(), tInv.getMzz());
        thetaY = Math.atan2(-1 * tInv.getMzx(), Math.sqrt((tInv.getMzy() * tInv.getMzy()) + (tInv.getMzz() * tInv.getMzz())));
        thetaZ = Math.atan2(tInv.getMyx(), tInv.getMxx());

        thetaX = Math.toDegrees(thetaX);
        thetaY = Math.toDegrees(thetaY);
        thetaZ = Math.toDegrees(thetaZ);

        // System.out.printf("%f %f %f\n", thetaX, thetaY, thetaZ);
        // System.out.printf("%f %f %f\n", box.getTranslateX(), box.getTranslateY(), box.getTranslateZ());

        // Rotate rotateBoxX, rotateBoxY, rotateBoxZ;
        rotateBoxX.setAngle(thetaX);
        rotateBoxY.setAngle(thetaY);
        rotateBoxZ.setAngle(thetaZ);
        
        // for (Node node : rubix.getChildren()) {
        //   node.getTransforms().clear();
        //   node.getTransforms().addAll(pivot, rotateBoxZ, rotateBoxY, rotateBoxX);
        // }

        t2 = new Rotate();
        t2 = t2.createConcatenation(rotateBoxZ);
        t2 = t2.createConcatenation(rotateBoxY);
        t2 = t2.createConcatenation(rotateBoxX);
        // box.getTransforms().clear();
        // box.getTransforms().addAll(pivot, t2);


      }

      if (me.isPrimaryButtonDown()) {
        if (nonAxis != null) {
          if (axis == null) {
            Point3D pX3 = t2.transform(Rotate.X_AXIS);
            Point3D pY3 = t2.transform(Rotate.Y_AXIS);
            Point3D pZ3 = t2.transform(Rotate.Z_AXIS);

            Point2D pX2Perp = new Point2D(Rotate.Y_AXIS.dotProduct(pX3), -1 * Rotate.X_AXIS.dotProduct(pX3));
            Point2D pY2Perp = new Point2D(Rotate.Y_AXIS.dotProduct(pY3), -1 * Rotate.X_AXIS.dotProduct(pY3));
            Point2D pZ2Perp = new Point2D(Rotate.Y_AXIS.dotProduct(pZ3), -1 * Rotate.X_AXIS.dotProduct(pZ3));

            // double pX2_slope = pX2Perp.getY() / pX2Perp.getX();
            // double pY2_slope = pY2Perp.getY() / pY2Perp.getX();
            // double pZ2_slope = pZ2Perp.getY() / pZ2Perp.getX();

            Point2D mouse = new Point2D(dx, dy);
            double xDot = Math.abs(pX2Perp.dotProduct(mouse));
            double yDot = Math.abs(pY2Perp.dotProduct(mouse));
            double zDot = Math.abs(pZ2Perp.dotProduct(mouse));

            // if ((xDot >= yDot) && (xDot >= zDot) && (nonAxis != Rotate.X_AXIS)) {
            //   axis = Rotate.X_AXIS;
            // } else if ((yDot >= xDot) && (yDot >= zDot) && (nonAxis != Rotate.Y_AXIS)) {
            //   axis = Rotate.Y_AXIS;
            // } else if ((zDot >= xDot) && (zDot >= yDot) && (nonAxis != Rotate.Z_AXIS)) {
            //   axis = Rotate.Z_AXIS;
            // } else {
            //   System.out.println("SOMETHING HAS GONE HORRIBLY WRONG: rotation is close to no axis");
            // }

            dots = new HashMap<>();
            dots.put(xDot, new Pair<>(Rotate.X_AXIS, pX2Perp));
            dots.put(yDot, new Pair<>(Rotate.Y_AXIS, pY2Perp));
            dots.put(zDot, new Pair<>(Rotate.Z_AXIS, pZ2Perp));
            TreeMap<Double, Pair<Point3D, Point2D>> sorted = new TreeMap<>();
            sorted.putAll(dots);

            for (Map.Entry<Double, Pair<Point3D, Point2D>> entry : sorted.entrySet()) {
              if (entry.getValue().getKey() == nonAxis) continue;
              axis = entry.getValue().getKey();
              dotsKey = entry.getKey();
              break;
            }

          
            rotateGroup = new Rotate();
            // rubix.getChildren().removeAll(groups.get(axis));
            // rotatingGroup.getChildren().addAll(groups.get(axis));
            // rotatingGroup.getTransforms().addAll(pivot, tInv, rotateGroup);
            for (Cubelet cubelet : groups.get(axis)) {
              cubelet.getTransforms().add(rotateGroup);
            }
          }

          totX += dx;
          totY += dy;

          double angle = dots.get(dotsKey).getValue().dotProduct(new Point2D(totX, totY));

          // groups.get(axis).setRotationAxis(axis);
          // groups.get(axis).setRotate(angle);

          rotateGroup.setAxis(axis);
          rotateGroup.setAngle(angle);

          // System.out.printf("%s %f\n", axis, angle);
        }

      }

      mousePosX = me.getSceneX();
      mousePosY = me.getSceneY();
    });
  }

  public void updateSides(Map<Point3D, ArrayList<Cubelet>> groups, Point3D axis, double angle) {
    List<Cubelet> cubeletsList = groups.get(axis);
    List<Side> sidesList = sides.get(axis);
    int rotates = (4 - (((int)Math.IEEEremainder((double)((int)angle / 90), 4.) + 4) % 4)) % 4;
    int swap;
    // System.out.printf("rotates: %d\n", rotates);

    // for (Cubelet cubelet : cubeletsList) {
    //   for (int i = 0; i < rotates; i++) {
    //     System.out.printf("rotate: %d/%d", i + 1, rotates);
    //     if (axis == Rotate.X_AXIS) {
    //       System.out.printf("old coords: %d %d %d, ", cubelet.i, cubelet.j, cubelet.k);
    //       rubix.cubelets[cubelet.i][cubelet.k][dim - cubelet.j - 1] = cubelet;
    //       swap = cubelet.j;
    //       cubelet.j = cubelet.k;
    //       cubelet.k = dim - swap - 1;
    //       System.out.printf("new coords: %d %d %d\n", cubelet.i, cubelet.j, cubelet.k);
    //     } else if (axis == Rotate.Y_AXIS) {
    //       rubix.cubelets[dim - cubelet.k - 1][cubelet.j][cubelet.i] = cubelet;
    //       swap = cubelet.k;
    //       cubelet.k = cubelet.i;
    //       cubelet.i = dim - swap - 1;
    //     } else if (axis == Rotate.Z_AXIS) {
    //       rubix.cubelets[dim - cubelet.j - 1][cubelet.i][cubelet.k] = cubelet;
    //       swap = cubelet.j;
    //       cubelet.j = cubelet.i;
    //       cubelet.i = dim - swap - 1;
    //     }
    //   }
    for (int i = 0; i < rotates; i++) {
      System.out.printf("rotate: %d/%d\n", i + 1, rotates);
      for (Cubelet cubelet : cubeletsList) {
        if (axis == Rotate.X_AXIS) {
          System.out.printf("old coords: %d %d %d, ", cubelet.i, cubelet.j, cubelet.k);
          rubix.cubelets[cubelet.i][cubelet.k][dim - cubelet.j - 1] = cubelet;
          swap = cubelet.j;
          cubelet.j = cubelet.k;
          cubelet.k = dim - swap - 1;
          System.out.printf("new coords: %d %d %d\n", cubelet.i, cubelet.j, cubelet.k);
        } else if (axis == Rotate.Y_AXIS) {
          rubix.cubelets[dim - cubelet.k - 1][cubelet.j][cubelet.i] = cubelet;
          swap = cubelet.k;
          cubelet.k = cubelet.i;
          cubelet.i = dim - swap - 1;
        } else if (axis == Rotate.Z_AXIS) {
          rubix.cubelets[dim - cubelet.j - 1][cubelet.i][cubelet.k] = cubelet;
          swap = cubelet.j;
          cubelet.j = cubelet.i;
          cubelet.i = dim - swap - 1;
        }
      }
    }
      // if (axis == Rotate.X_AXIS) {
      //   for (Cubelet cubelet : cubeletsList) {
      //     for (int i = 0; i < rotates; i++) {
      //       rubix.cubelets[cubelet.i][cubelet.k][dim - cubelet.j - 1] = cubelet;
      //     }
      //   }
      // }
      // if (axis == Rotate.Y_AXIS) {
      //   for (Cubelet cubelet : cubeletsList) {
      //     for (int i = 0; i < rotates; i++) {
      //       rubix.cubelets[dim - cubelet.k - 1][cubelet.j][cubelet.i] = cubelet;
      //     }
      //   }
      // }
      // if (axis == Rotate.Z_AXIS) {
      //   for (Cubelet cubelet : cubeletsList) {
      //     for (int i = 0; i < rotates; i++) {
      //       rubix.cubelets[dim - cubelet.j - 1][cubelet.i][cubelet.k] = cubelet;
      //     }
      //   }
      // }
    for (Cubelet cubelet : cubeletsList) {
      for (Node node : cubelet.getChildren()) {
        if (node instanceof CBox) {
          int index = sidesList.indexOf(((CBox)node).getSide());
          if (index != -1) {
            ((CBox)node).setSide(sidesList.get(((int)Math.IEEEremainder((double)(index + ((int)angle / 90)), 4.) + 4) % 4));
          }
        }
      }
    }
  }

  public static void main(String[] args) {
    launch();
  }

}
// i j->    Z_AXIS
// |  c c c 
//    c c c 
//    c c c

//   c(0, 0, 0) -> c(2, 0, 0)
//   c(1, 2, 1) -> c(0, 1, 1)

// k i->    Y_AXIS
// |  c c c 
//    c c c 
//    c c c

// c(0, 0, 0) -> c(2, 0, 0)
// c(1, 0, 2) -> c(0, 0, 1)

// j k->    X_AXIS
// |  c c c 
//    c c c 
//    c c c

// c(0, 0, 0) -> c(0, 0, 2)
// c(1, 1, 2) -> c(1, 2, 1)