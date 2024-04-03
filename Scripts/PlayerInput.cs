using UnityEngine;
using UnityEngine.UI;


//---------------
// Mouse logic:
// - Left&Right:    if(tile is revealed)        Reveal Neighbors
// - Left click:    if(not after Left&Right)    Reveal Tile
// - Right click:   if(not after Left&Right)    Flag Tile

public class PlayerInput : MonoBehaviour {

    public Sprite sp_on_flag;
    public Sprite sp_off_flag;
    public Image img_icon_flag;

    // static variables
    private static bool _rightAndLeftPressed;
    private static bool _revealAreaIssued;
    private static bool _initialClickIssued;

    private bool is_flag = false;

    // private variables
    private GridScript _grid;

    // handles
    public UIManager UI;
    public Canvas PauseMenu;

    // getters & setters
    public GridScript Grid
    {
        get { return _grid; }
        set { _grid = value; }
    }

    public static bool InitialClickIssued
    {
        get { return _initialClickIssued; }
        set
        {
            _initialClickIssued = value;
        }
    }

    public void OnMouseOver(Tile tile)
    {
       
        // RIGHT CLICK: FLAG
        if (!_revealAreaIssued && Input.GetMouseButtonDown(1))
        {
            if(!Input.GetMouseButton(0) && !tile.IsRevealed())
            {
                GameObject.Find("GameManager").GetComponent<GameManager>().play_sound(4);
                tile.ToggleFlag();
            }    
        }

        // LEFT CLICK: HIGHLIGHT TILE
        if (Input.GetMouseButton(0))
        {
            if(!tile.IsRevealed() && !tile.IsFlagged())
            {
                if (this.is_flag)
                {
                    GameObject.Find("GameManager").GetComponent<GameManager>().play_sound(4);
                    tile.ToggleFlag();
                }
                else
                {
                    _grid.HighlightTile(tile.GridPosition);
                    GameObject.Find("GameManager").GetComponent<GameManager>().play_sound(3);
                }
            }
                
            
            // LEFT & RIGHT CLICK: HIGHLIGHT AREA
            if (Input.GetMouseButton(1))
            {
                _rightAndLeftPressed = true;
            }
        }

        // LEFT & RIGHT RELEASE: REVEAL NEIGHBORS IF ENOUGH NEIGHBOR FLAGGED
        if (_rightAndLeftPressed) this.act_set_flag(tile);

        if (Input.GetMouseButtonUp(0) && !_revealAreaIssued)
        {
            if (!tile.IsFlagged() && !tile.IsRevealed())
            {
                if (!_initialClickIssued)
                {
                    if (tile.IsMine())
                        _grid.SwapTileWithMineFreeTile(tile.GridPosition);

                    _initialClickIssued = true;
                    GetComponent<GameManager>().StartTimer();
                    tile.Reveal();
                }

                else
                {
                    if(!Input.GetMouseButton(1)) tile.Reveal();
                }
            }    
        }


        if (!Input.GetMouseButton(0) || !Input.GetMouseButton(1))
        {
            _rightAndLeftPressed = false;
        }

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            _revealAreaIssued = false;
        }
    }

    private void on_flag()
    {
        this.img_icon_flag.sprite = this.sp_off_flag;
        this.is_flag = true;
    }

    private void off_flag()
    {
        this.img_icon_flag.sprite = this.sp_on_flag;
        this.is_flag = false;
    }

    public void btn_change_flag()
    {
        if (this.is_flag)
            this.off_flag();
        else
            this.on_flag();
    }

    private void act_set_flag(Tile tile)
    {
        _grid.HighlightArea(tile.GridPosition);
        if (!tile.IsRevealed() && !tile.IsFlagged()) tile.Highlight();
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            _revealAreaIssued = true;
            if (tile.IsRevealed() && tile.IsNeighborsFlagged())
            {
                _grid.RevealArea(tile);
            }
            else
            {
                _grid.RevertHighlightArea(tile.GridPosition);
                _grid.RevertHighlightTile(tile.GridPosition);
            }
        }
    }

    public void OnMouseExit(Tile tile)
    {
        if(!tile.IsRevealed() && !tile.IsFlagged())  tile.RevertHighlight();

        foreach (Vector2 pos in tile.NeighborTilePositions)
        {
            Tile neighbor = _grid.Map[(int) pos.x][(int) pos.y];
            if (!neighbor.IsRevealed() && !neighbor.IsFlagged())
                neighbor.RevertHighlight();
        }
    }

}
