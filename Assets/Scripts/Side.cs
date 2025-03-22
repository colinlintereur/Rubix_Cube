using UnityEngine;

public enum Side
{
  FRONT,
  BACK,
  LEFT,
  RIGHT,
  UP,
  DOWN,
  INSIDE
}

public static class SideExtensions
{
  public static Vector3 GetNonAxis(this Side side) => side switch
  {
    Side.FRONT => Vector3.forward,
    Side.BACK => Vector3.forward,
    Side.LEFT => Vector3.right,
    Side.RIGHT => Vector3.right,
    Side.UP => Vector3.up,
    Side.DOWN => Vector3.up,
    Side.INSIDE => Vector3.zero,
    _ => Vector3.zero,
  };
}
