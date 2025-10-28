using UnityEngine;
using UnityEngine.InputSystem;

public class move_arm : MonoBehaviour
{
    Vector2 movement;
    public float rotateSpeed = 80.0f;
    public GameObject boneReferenceX;
    public GameObject boneReferenceY;
    public GameObject handBoneToCheckClipping;
    public bool isHand;

    private float xActual = 0.0f;
    private float yActual = 0.0f;

    void Move(Vector2 movement)
    {
        // los ejes que de Vector3 son para los locales del hueso

        if (isHand) //movemos la mano desde la muñeca
        {
            float y_rotAmount = movement.y * rotateSpeed * Time.deltaTime;
            // limites en y -20 y 25
            float y_new = Mathf.Clamp(yActual + y_rotAmount, -60.0f, 50.0f); //limites reales: -30, 25 (se multiplican x2 porque el rotation constrain hace la media entre ambos objetos de manera que para llegar a 30 se hace la media de: (60+0)/2 )
            // la diferencia entre donde esta la rotacion y a donde quiere rotar, si ya esta en el limite será 0 y no rotará
            float y_diff = y_new - yActual;


            float x_rotAmount = movement.x * rotateSpeed * Time.deltaTime;
            float x_new = Mathf.Clamp(xActual + x_rotAmount, -140.0f, 170.0f); //limites reales: -70, 85
            float x_diff = x_new - xActual; // la diferencia entre donde esta la rotacion y a donde quiere rotar, si ya esta en el limite será 0 y no rotará


            boneReferenceY.transform.Rotate(Vector3.forward, y_diff);
            boneReferenceX.transform.Rotate(Vector3.right, x_diff);
            // forward para rotar paralelo a la palma (como gesticulando "hola")
            // right para rotar perpendicular a la palma

            yActual = y_new;
            xActual = x_new;


        } else // movemos el brazo desde el codo
        {
            float y_rotAmount = movement.y * rotateSpeed * Time.deltaTime;
            // limites en y -20 y 25
            float y_new = Mathf.Clamp(yActual + y_rotAmount, -90.0f, 100.0f); //limites reales: -45, 50
            // la diferencia entre donde esta la rotacion y a donde quiere rotar, si ya esta en el limite será 0 y no rotará
            float y_diff = y_new - yActual;


            float x_rotAmount = movement.x * rotateSpeed * Time.deltaTime;
            float x_new = Mathf.Clamp(xActual + x_rotAmount, -160.0f, 160.0f); //limites reales: -80, 80
            float x_diff = x_new - xActual; // la diferencia entre donde esta la rotacion y a donde quiere rotar, si ya esta en el limite será 0 y no rotará

            boneReferenceY.transform.Rotate(Vector3.forward, y_diff);
            boneReferenceX.transform.Rotate(Vector3.right, x_diff);
            // forward para flexionar el codo
            // right para rotar el codo

            yActual = y_new;
            xActual = x_new;
        }
    }

    void Update()
    {
        // leemos el valor del joystick (0-1)
        var gamepad = Gamepad.current;
        if (gamepad == null)
        {
            return; // no hay gamepad
        }

        if (isHand)
        {
            movement = gamepad.rightStick.ReadValue(); //joystick derecho controla la mano
        } else
        {
            movement = gamepad.leftStick.ReadValue(); //joystick izquierdo controla el brazo
        }

        Move(movement);
    }  

    void funcionParaGuardarCosas()
    {
    
    }
}
