using OpenTK.Mathematics;

namespace Minecraft;

public class Camera
{
    public Vector3 Position { get; set; } = new Vector3(64, 32, 8);
    public Vector3 Rotation { get; set; } = new Vector3(0, 0, 0);
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; } = Vector3.UnitX;
    
    public Matrix4 View { get; private set; }
    public Matrix4 Projection { get; private set; }
    
    private float _yaw = -90.0f;
    private float _pitch = 0.0f;
    private float _movementSpeed = 0.01f;
    private float _mouseSensitivity = 0.1f;

    public Camera(int width, int height)
    {
        Projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45.0f),
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
            MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(_pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch))
        );
        Front = Vector3.Normalize(front);
        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        
        View = Matrix4.LookAt(Position, Position + Front, Up);
    }

    public void ProcessMouseMovement(float deltaX, float deltaY)
    {
        _yaw -= deltaX * _mouseSensitivity;
        _pitch += deltaY * _mouseSensitivity;

        // Ограничение угла наклона камеры
        _pitch = Math.Clamp(_pitch, -89.0f, 89.0f);
        
        UpdateViewMatrix();
    }

    public void Move(Vector3 direction)
    {
        Position += direction * _movementSpeed;
        UpdateViewMatrix();
    }

    public void Rotate(Vector3 rotationDelta)
    {
        _yaw += rotationDelta.X;
        _pitch += rotationDelta.Y;
        _pitch = Math.Clamp(_pitch, -89.0f, 89.0f);
        UpdateViewMatrix();
    }
}
