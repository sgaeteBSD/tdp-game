using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private GameObject healthObj;
    [SerializeField] private List<GameObject> hp;
    [SerializeField] private List<GameObject> lostHp;

    private float xVal = 18.966f;
    private float yVal = -43.10349f;
    private float xOffset = 20.69f;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject health = Instantiate(healthObj, transform);
            hp.Add(health);
            health.GetComponent<RectTransform>().anchoredPosition = new Vector2(xVal + (xOffset * i), yVal);
        }

        StartCoroutine("HPBump");
    }

    public void Damaged()
    {
        hp[hp.Count - 1].GetComponent<Animator>().Play("HPLoss");
        lostHp.Add(hp[hp.Count - 1]);
        hp.Remove(hp[hp.Count - 1]);
        if (hp.Count <= 0)
        {
            Debug.Log("dead...");
        }
    }

    IEnumerator HPBump()
    {
        while (true)
        {
            foreach (var health in hp)
            {
                health.GetComponent<Animator>().Play("HPBump");
                yield return new WaitForSeconds(0.1f);
            }
            
            yield return new WaitForSeconds(10f);

            foreach (var health in hp)
            {
                health.GetComponent<Animator>().Play("HPIdle");
            }
        }
    }

    
}
