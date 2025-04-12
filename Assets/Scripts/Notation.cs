using System;
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
  D_,
  M,   // Middle
  M_,
  E,   // Middle
  E_,
  S,   // Middle
  S_,
  X,   // Rotate the whole cube
  X_,
  Y,   // Rotate the whole cube
  Y_,
  Z,   // Rotate the whole cube
  Z_,
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
    Notation.M => Side.FRONT,
    Notation.M_ => Side.FRONT,
    Notation.E => Side.FRONT,
    Notation.E_ => Side.FRONT,
    Notation.S => Side.UP,
    Notation.S_ => Side.UP,
    Notation.X => Side.FRONT,
    Notation.X_ => Side.FRONT,
    Notation.Y => Side.FRONT,
    Notation.Y_ => Side.FRONT,
    Notation.Z => Side.FRONT,
    Notation.Z_ => Side.FRONT,
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
    Notation.M => Vector3.right,
    Notation.M_ => Vector3.right,
    Notation.E => Vector3.up,
    Notation.E_ => Vector3.up,
    Notation.S => Vector3.forward,
    Notation.S_ => Vector3.forward,
    Notation.X => Vector3.right,
    Notation.X_ => Vector3.right,
    Notation.Y => Vector3.up,
    Notation.Y_ => Vector3.up,
    Notation.Z => Vector3.forward,
    Notation.Z_ => Vector3.forward,
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
    Notation.M => -1,
    Notation.M_ => 1,
    Notation.E => -1,
    Notation.E_ => 1,
    Notation.S => -1,
    Notation.S_ => 1,
    Notation.X => 1,
    Notation.X_ => -1,
    Notation.Y => 1,
    Notation.Y_ => -1,
    Notation.Z => -1,
    Notation.Z_ => 1,
    _ => 0,
  };

  public static bool IsCubeRotation(this Notation notation) => notation switch
  {
    Notation.F => false,
    Notation.F_ => false,
    Notation.R => false,
    Notation.R_ => false,
    Notation.U => false,
    Notation.U_ => false,
    Notation.L => false,
    Notation.L_ => false,
    Notation.B => false,
    Notation.B_ => false,
    Notation.D => false,
    Notation.D_ => false,
    Notation.M => false,
    Notation.M_ => false,
    Notation.E => false,
    Notation.E_ => false,
    Notation.S => false,
    Notation.S_ => false,
    Notation.X => true,
    Notation.X_ => true,
    Notation.Y => true,
    Notation.Y_ => true,
    Notation.Z => true,
    Notation.Z_ => true,
    _ => false,
  };
}

public static class Notations
{
  public static Notation ConvertString(String str) => str switch
  {
    "F" => Notation.F,
    "F'" => Notation.F_,
    "R" => Notation.R,
    "R'" => Notation.R_,
    "U" => Notation.U,
    "U'" => Notation.U_,
    "L" => Notation.L,
    "L'" => Notation.L_,
    "B" => Notation.B,
    "B'" => Notation.B_,
    "D" => Notation.D,
    "D'" => Notation.D_,
    "M" => Notation.M,
    "M'" => Notation.M_,
    "E" => Notation.E,
    "E'" => Notation.E_,
    "S" => Notation.S,
    "S'" => Notation.S_,
    "X" => Notation.X,
    "X'" => Notation.X_,
    "Y" => Notation.Y,
    "Y'" => Notation.Y_,
    "Z" => Notation.Z,
    "Z'" => Notation.Z_,
    _ => Notation.UNSET
  };
}