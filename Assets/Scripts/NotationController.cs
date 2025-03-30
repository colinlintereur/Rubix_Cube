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

}