using OpenTK.Mathematics;

namespace Minecraft;

public class Camera
{
    public Vector3 Position { get; set; } = new Vector3(0, 0, 1);
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; } = Vector3.UnitX;
    
    public Matrix4 View { get; private set; }
    public Matrix4 Projection { get; private set; }

    public float Yaw { get; private set; }
    public float Pitch { get; private set; }
    public float MovementSpeed = 0.02f;
    private float _mouseSensitivity = 0.1f;

    public Camera(int width, int height)
    {
        Projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(90.0f),
            (float)width / height,
            0.1f,
            1000.0f
        );
        
        UpdateViewMatrix();
    }

    public void UpdateViewMatrix()
    {
        // Вычисление направления взгляда
        Vector3 front = new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(Pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch))
        );
        Front = Vector3.Normalize(front);
        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        
        View = Matrix4.LookAt(Position, Position + Front, Up);
    }

    public void ProcessMouseMovement(float deltaX, float deltaY)
    {
        Yaw -= deltaX * _mouseSensitivity;
        Pitch += deltaY * _mouseSensitivity;

        // Ограничение угла наклона камеры
        Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);
        
        UpdateViewMatrix();
    }

    public void Move(Vector3 direction)
    {
        Position += direction * MovementSpeed;
        UpdateViewMatrix();
    }

    public void Rotate(Vector3 rotationDelta)
    {
        Yaw += rotationDelta.X;
        Pitch += rotationDelta.Y;
        Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);
        UpdateViewMatrix();
    }
}
