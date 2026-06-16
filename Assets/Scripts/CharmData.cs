using UnityEngine;

// Liste propre de vos charmes (facile à étendre par la suite)
public enum CharmType
{
    None,
    BlobsEye,
    WitheredClover,
    CursedBandages
}

[CreateAssetMenu(fileName = "New Charm", menuName = "Charms/Charm Data")]
public class CharmData : ScriptableObject
{
    public string charmName;
    public Sprite charmSprite;
    [TextArea] public string description;
    public CharmType charmType;
}