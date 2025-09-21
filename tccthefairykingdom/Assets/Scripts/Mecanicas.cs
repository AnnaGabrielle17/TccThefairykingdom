using UnityEngine;

public class Mecanicas : MonoBehaviour
{
    public float velocidadeDaMecanica;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        MovimentarMecanica();
    }
    private void MovimentarMecanica()
    {
        transform.Translate(Vector3.left * velocidadeDaMecanica * Time.deltaTime);
    }

}
