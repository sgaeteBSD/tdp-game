using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [SerializeField] private float iconSize = 10f;
    public GameObject minimapPanel; //UI panel for minimap
    public GameObject mapIconPrefab; //room icon image prefab
    public RectTransform minimapGrid; //must NOT be a real grid layout :( but it lets us use roomGen's coordinates
    public RoomGenerator roomGenerator; //ref to roomGen
    private bool mapGen = false;

    void Start()
    {
        minimapPanel.SetActive(false); //start with map hidden
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            minimapPanel.SetActive(!minimapPanel.activeSelf);
            if (!mapGen)
            {
                GenerateMap();
                mapGen = true;
            }
        }
    }

    void GenerateMap()
    {
        foreach (var roomPos in roomGenerator.placedRooms)
        {
            GameObject icon = Instantiate(mapIconPrefab, minimapGrid);
            Image iconImage = icon.GetComponent<Image>();

            //set colors based on room type
            if (roomPos == roomGenerator.center)
                iconImage.color = Color.blue; //starting room
            else if (roomGenerator.deadEnds.Contains(roomPos))
                iconImage.color = Color.red; //dead-end room
            else
                iconImage.color = Color.white; //normal room

            //correct position on the minimap
            RectTransform iconTransform = icon.GetComponent<RectTransform>();
            Vector2 minimapPosition = ConvertToMinimapPosition(roomPos);
            iconTransform.anchoredPosition = minimapPosition; //here
            iconTransform.sizeDelta = new Vector2(iconSize, iconSize);
        }
    }

    Vector2 ConvertToMinimapPosition(Vector2Int roomPos)
    {
        float cellSize = 20f; //adjust based on UI scaling
        Vector2Int center = roomGenerator.center; //get center from room generator

        //offset position relative to minimap center
        int xOffset = roomPos.x - center.x;
        int yOffset = roomPos.y - center.y;

        //convert to UI coordinates
        Vector2 uiPosition = new Vector2(xOffset * cellSize, yOffset * cellSize);

        return uiPosition;
    }
}
