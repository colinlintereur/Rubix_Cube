using UnityEngine;

// Letters refer to the face that is rotating (ex: F = Front).
// These faces are rotated CW from the perspective of looking at the rotation face head-on.
// Note: This is opposite to the original implementation.
// Letters with an underscore afterward are the same except they are rotated CCW
public enum Notation
{
  UNSET,
  F,   // Front
  F_,
  R,   // Right
  R_,
  U,   // Up
  U_,
  L,   // Left
  L_,
  B,   // Back
  B_,
  D,   // Down
  D_
}
public static class NotationExtensions
{
  public static Side GetSide(this Notation notation) => notation switch
  {
    Notation.F => Side.FRONT,
    Notation.F_ => Side.FRONT,
    Notation.R => Side.RIGHT,
    Notation.R_ => Side.RIGHT,
    Notation.U => Side.UP,
    Notation.U_ => Side.UP,
    Notation.L => Side.LEFT,
    Notation.L_ => Side.LEFT,
    Notation.B => Side.BACK,
    Notation.B_ => Side.BACK,
    Notation.D => Side.DOWN,
    Notation.D_ => Side.DOWN,
    _ => Side.INSIDE,
  };

  public static Vector3 GetRotateAxis(this Notation notation) => notation switch
  {
    Notation.F => Vector3.forward,
    Notation.F_ => Vector3.forward,
    Notation.R => Vector3.right,
    Notation.R_ => Vector3.right,
    Notation.U => Vector3.up,
    Notation.U_ => Vector3.up,
    Notation.L => Vector3.right,
    Notation.L_ => Vector3.right,
    Notation.B => Vector3.forward,
    Notation.B_ => Vector3.forward,
    Notation.D => Vector3.up,
    Notation.D_ => Vector3.up,
    _ => Vector3.zero,
  };

  public static int GetRotationSign(this Notation notation) => notation switch
  {
    Notation.F => -1,
    Notation.F_ => 1,
    Notation.R => 1,
    Notation.R_ => -1,
    Notation.U => 1,
    Notation.U_ => -1,
    Notation.L => -1,
    Notation.L_ => 1,
    Notation.B => 1,
    Notation.B_ => -1,
    Notation.D => -1,
    Notation.D_ => 1,
    _ => 0,
  };
}