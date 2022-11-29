using UnityEngine;

public class RandomObjectColor : MonoBehaviour
{
    [SerializeField] bool useRandomColor = true;
    [SerializeField] Color color;

    // Start is called before the first frame update
    void Awake()
    {
        ApplyRandomColorMaterialToPlayer();
    }

    private void ApplyRandomColorMaterialToPlayer()
    {
        if (useRandomColor)
        {
            color = new Color(Random.Range(0, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }
        gameObject.GetComponent<MeshRenderer>().material.color = color;
    }
}
