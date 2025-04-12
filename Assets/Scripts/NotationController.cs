using UnityEngine;

public class NotationController : MonoBehaviour
{
  CubeController cubeController;

  // Start is called before the first frame update
  void Start()
  {
    cubeController = GameObject.Find("Controller").GetComponent<CubeController>();
  }

  // Called by the F.onClick()
  public void F()
  {
    cubeController.NotationRotate(Notation.F);
  }

  // Called by the R.onClick()
  public void R()
  {
    cubeController.NotationRotate(Notation.R);
  }

  // Called by the U.onClick()
  public void U()
  {
    cubeController.NotationRotate(Notation.U);
  }

  // Called by the L.onClick()
  public void L()
  {
    cubeController.NotationRotate(Notation.L);
  }

  // Called by the B.onClick()
  public void B()
  {
    cubeController.NotationRotate(Notation.B);
  }

  // Called by the D.onClick()
  public void D()
  {
    cubeController.NotationRotate(Notation.D);
  }
  // Called by the F'.onClick()
  public void F_()
  {
    cubeController.NotationRotate(Notation.F_);
  }

  // Called by the R'.onClick()
  public void R_()
  {
    cubeController.NotationRotate(Notation.R_);
  }

  // Called by the U'.onClick()
  public void U_()
  {
    cubeController.NotationRotate(Notation.U_);
  }

  // Called by the L'.onClick()
  public void L_()
  {
    cubeController.NotationRotate(Notation.L_);
  }

  // Called by the B'.onClick()
  public void B_()
  {
    cubeController.NotationRotate(Notation.B_);
  }

  // Called by the D'.onClick()
  public void D_()
  {
    cubeController.NotationRotate(Notation.D_);
  }

  // Called by the M.onClick()
  public void M()
  {
    cubeController.NotationRotate(Notation.M);
  }

  // Called by the E.onClick()
  public void E()
  {
    cubeController.NotationRotate(Notation.E);
  }

  // Called by the S.onClick()
  public void S()
  {
    cubeController.NotationRotate(Notation.S);
  }

  // Called by the M'.onClick()
  public void M_()
  {
    cubeController.NotationRotate(Notation.M_);
  }

  // Called by the E'.onClick()
  public void E_()
  {
    cubeController.NotationRotate(Notation.E_);
  }

  // Called by the S'.onClick()
  public void S_()
  {
    cubeController.NotationRotate(Notation.S_);
  }

  // Called by the X.onClick()
  public void X()
  {
    cubeController.NotationRotate(Notation.X);
  }

  // Called by the Y.onClick()
  public void Y()
  {
    cubeController.NotationRotate(Notation.Y);
  }

  // Called by the Z.onClick()
  public void Z()
  {
    cubeController.NotationRotate(Notation.Z);
  }

  // Called by the X'.onClick()
  public void X_()
  {
    cubeController.NotationRotate(Notation.X_);
  }

  // Called by the Y'.onClick()
  public void Y_()
  {
    cubeController.NotationRotate(Notation.Y_);
  }

  // Called by the Z'.onClick()
  public void Z_()
  {
    cubeController.NotationRotate(Notation.Z_);
  }
}