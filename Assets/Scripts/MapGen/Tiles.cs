using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tiles : MonoBehaviour
{
    public static Tiles instance;

    [Header("Ground tiles")]
    [SerializeField] private Tile basicGroundTile = default;
    [SerializeField] private Tile crackedGroundTile = default;
    [SerializeField] private Tile crossGroundTile = default;
    [Header("Wall tiles")]
    [SerializeField] private Tile leftWallTile = default;
    [SerializeField] private Tile rightWallTile = default;
    [SerializeField] private Tile bottomWallTile = default;
    [SerializeField] private Tile topWallTile = default;
    [Space]
    [SerializeField] private Tile bottomLeftCornerVoid = default;
    [SerializeField] private Tile topLeftCornerVoid = default;
    [SerializeField] private Tile topRightCornerVoid = default;
    [SerializeField] private Tile bottomRightCornerVoid = default;
    [SerializeField] private Tile bottomLeftCornerNoVoid = default;
    [SerializeField] private Tile topLeftCornerNoVoid = default;
    [SerializeField] private Tile topRightCornerNoVoid = default;
    [SerializeField] private Tile bottomRightCornerNoVoid = default;
    [Space]
    [SerializeField] private Tile brickWallTile = default;
    [Header("Ground tile distribution percentage")]
    [SerializeField] private float basicTile = default;
    [SerializeField] private float crackedTile = default;

    private float basicChance;
    private float crackedChance;

    private void Awake()
    {
        instance = this;
        basicChance = basicTile;
        crackedChance = basicChance + crackedTile;
    }

    public Tile GetGroundTile()
    {
        int ran = Random.Range(0, 100);
        if(ran < basicChance)
            return basicGroundTile;
        else if(ran < crackedChance)
            return crackedGroundTile;
        else
            return crossGroundTile;
    }

    public Tile GetBasicGroundTile()
    {
        return basicGroundTile;
    }

    public Tile GetLeftWall()
    {
        return leftWallTile;
    }

    public Tile GetRightWall()
    {
        return rightWallTile;
    }

    public Tile GetBottomWall()
    {
        return bottomWallTile;
    }

    public Tile GetTopWall()
    {
        return topWallTile;
    }

    public Tile GetBrickWall()
    {
        return brickWallTile;
    }

    public Tile GetTopLeftCorner(bool isVoid)
    {
        return isVoid ? topLeftCornerVoid : topLeftCornerNoVoid;
    }   

    public Tile GetTopRightCorner(bool isVoid)
    {
        return isVoid ? topRightCornerVoid : topRightCornerNoVoid;
    }

    public Tile GetBottomLeftCorner(bool isVoid)
    {
        return isVoid ? bottomLeftCornerVoid : bottomLeftCornerNoVoid;
    }

    public Tile GetBottomRightCorner(bool isVoid)
    {
        return isVoid ? bottomRightCornerVoid : bottomRightCornerNoVoid;
    }
}
