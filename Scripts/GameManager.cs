using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum game_mode {easy,dificult,super_difficult}
public class GameManager : MonoBehaviour
{

    [Header("Panel Game")]
    public GameObject panel_menu;
    public GameObject panel_play;
    public GameObject panel_done;
    public GameObject panel_win;
    public GameObject panel_gameover;
    public Text txt_done_hight_score;

    [Header("Obj Game")]
    public static bool IsGamePaused;
    public static bool IsGameOver;

    public Carrot.Carrot carrot;
    // handles
    public GameObject GridPrefab;
    public UIManager UI;
    public GameObject cam_skybox;
    public Camera cam_game;
    private GridScript _grid;
    public ParticleSystem[] Explosions;
    public PlayerInput player_input;

    // private variables
    private GameObject grid_game;
    private Transform _gridtf;
    private GameSettings _settings;
    private game_mode play_mode;

    // score variables
    private float _startTime;
    private float _endTime;
    private int _flagCount;
    private int game_score_hight = 0;

    [Header("sounds")]
    public AudioClip click_sound_clip;
    public AudioSource[] sound;

    public GameSettings Settings
    {
        get { return _settings; }
        set { _settings = value; }
    }


    [Header("Skyboxes")]
    public Material[] Skyboxes;

    void Awake()
    {
        _settings = new GameSettings();
        if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().Get_status_portrait())
            _settings = GameSettings.Beginner_portrait;
        else
            _settings = GameSettings.Beginner_landspace;
        
    }

    void Start ()
    {
        this.carrot.Load_Carrot(this.check_exit_app);

        this.game_score_hight = PlayerPrefs.GetInt("game_score_hight",0);
        this.carrot.change_sound_click(this.click_sound_clip);
        this.carrot.game.load_bk_music(this.sound[0]);

        this.panel_menu.SetActive(true);
        this.panel_play.SetActive(false);
        this.panel_done.SetActive(false);
        this.load_skybox();
    }

    private void check_exit_app()
    {
        if (this.panel_play.activeInHierarchy)
        {
            this.btn_back_menu();
            this.carrot.set_no_check_exit_app();
        }
    }

    private void Update()
    {
        UI.UpdateFlagText(_flagCount);
        if (PlayerInput.InitialClickIssued && !IsGamePaused && !IsGameOver)  UI.UpdateTimeText((int) (Time.time - _startTime));
    }

    private void act_play_game()
    {
        this.carrot.ads.show_ads_Interstitial();
        this.panel_menu.SetActive(false);
        this.panel_play.SetActive(true);
        this.panel_done.SetActive(false);
        this.load_skybox();
        this.carrot.play_sound_click();
    }

    public void btn_game_play_easy()
    {
        this.play_mode = game_mode.easy;
        if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().Get_status_portrait())
        {
            this.StartNewGame(GameSettings.Beginner_portrait);
            this.cam_game.fieldOfView = 45f;
        }
        else
        {
            this.StartNewGame(GameSettings.Beginner_landspace);
            this.cam_game.fieldOfView = 25f;
        }

        this.act_play_game();
        
    }

    public void btn_game_play_dificult()
    {
        this.play_mode = game_mode.dificult;
        if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().Get_status_portrait())
        {
            this.StartNewGame(GameSettings.Intermediate_portrait);
            this.cam_game.fieldOfView = 50f;
        }  
        else
        {
            this.StartNewGame(GameSettings.Intermediater_landspace);
            this.cam_game.fieldOfView = 40f;
        }
            
        this.act_play_game();
    }

    public void btn_game_play_super_difficult()
    {
        this.play_mode = game_mode.super_difficult;
        if (this.GetComponent<Carrot.Carrot_DeviceOrientationChange>().Get_status_portrait())
        {
            this.StartNewGame(GameSettings.Expert_portrait);
            this.cam_game.fieldOfView = 60f;
        }
        else
        {
            this.StartNewGame(GameSettings.Expertr_landspace);
            this.cam_game.fieldOfView = 43f;
        }
            
        this.act_play_game();
        
    }

    public void btn_game_reset()
    {
        if (this.play_mode == game_mode.easy) this.btn_game_play_easy();
        if (this.play_mode == game_mode.dificult) this.btn_game_play_dificult();
        if (this.play_mode == game_mode.super_difficult) this.btn_game_play_super_difficult();
    }

    private void act_game_done()
    {
        this.panel_done.SetActive(true);
        this.panel_play.SetActive(false);
        this.panel_win.SetActive(false);
        this.panel_gameover.SetActive(false);
    }

    public void show_win()
    {
        this.carrot.ads.show_ads_Interstitial();
        this.act_game_done();
        this.panel_win.SetActive(true);
        this.carrot.game.update_scores_player(this.game_score_hight);
        this.play_sound(1);
    }

    public void show_gameover()
    {
        this.carrot.ads.show_ads_Interstitial();
        this.act_game_done();
        this.panel_gameover.SetActive(true);
        this.panel_play.SetActive(false);
        this.play_sound(2);
        this.carrot.play_vibrate();
    }

    private void load_skybox()
    {
        int rand_skybox = Random.Range(0, this.Skyboxes.Length);
        this.cam_skybox.GetComponent<Skybox>().material = this.Skyboxes[rand_skybox];
        this.cam_skybox.GetComponent<SkyboxScript>().rotation = GetRandomVector();
    }

    public void btn_back_menu()
    {
        this.carrot.ads.show_ads_Interstitial();
        this.carrot.play_sound_click();
        this.panel_play.SetActive(false);
        this.panel_menu.SetActive(true);
        this.grid_game.SetActive(false);
        this.panel_done.SetActive(false);
    }

    public void StartNewGame(GameSettings settings)
    {
        if(this.grid_game!=null) Destroy(this.grid_game);
        this.grid_game=Instantiate(GridPrefab, new Vector3(0, -180f, 0), Quaternion.identity);
        _gridtf = this.grid_game.transform;
        _grid = _gridtf.GetComponent<GridScript>();

        _settings = settings;
        _grid.GenerateMap(_settings);

        GetComponent<PlayerInput>().Grid = _grid;
        
        ResetGameState();
        UI.ResetHUD(_flagCount);

        this.cam_skybox.GetComponent<SkyboxScript>().rotation = GetRandomVector();
    }

    Vector3 GetRandomVector()
    {
        Vector3 v = new Vector3();
        int rnd = Random.Range(-3, 3);

        if (rnd == -3)
            v.z = -1;
        if (rnd == -2)
            v.y = -1;
        if (rnd == -1)
            v.x = -1;
        if (rnd == 0)
            v.x = 1;
        if (rnd == 1)
            v.y = 1;
        if (rnd == 2)
            v.z = 1;

        v *= 0.025f;

        return v;
    }

    public void StartTimer()
    {
        _startTime = Time.time;
    }

    public void GameOver(bool win)
    {
        IsGameOver = true;

        _grid.RevealMines();

        // HUD
        UI.HUD.GameStateText.enabled = true;
        UI.HUD.GameStateText.text = "Game: " + (win ? " Won" : " Lost");
        _endTime = Time.time - _startTime;
        Debug.Log("GAME WON:" + win + " | GAME ENDED IN " + _endTime + " SECONDS.");
        // score
        IsGamePaused = true;

        if (win)
        {
            this.show_win();
            this.game_score_hight++;
            PlayerPrefs.SetInt("game_score_hight", this.game_score_hight);
        }
        else
        {
            this.show_gameover();
        }

        this.txt_done_hight_score.text = this.game_score_hight.ToString();

    }

    public void UpdateFlagCounter(bool condition)
    {
        _flagCount += condition ? -1 : 1;
    }

    private void ResetGameState()
    {
        PlayerInput.InitialClickIssued = false;
        IsGamePaused = false;
        IsGameOver = false;
        _flagCount = _settings.Mines;
    }

    public void Detonate(Tile tile)
    {
        int index = Random.Range(0, Explosions.Length);
        Explosions[index].transform.position = tile.transform.position + new Vector3(0, 1, 0);
        Explosions[index].Play();
    }

    public void btn_game_setting()
    {
        this.carrot.ads.show_ads_Interstitial();
        Carrot.Carrot_Box box_setting=this.carrot.Create_Setting();
        box_setting.update_color_table_row();
    }

    public void btn_show_user()
    {
        this.carrot.user.show_login();
    }

    public void btn_show_rate()
    {
        this.carrot.show_rate();
    }

    public void btn_show_share()
    {
        this.carrot.show_share();
    }

    public void btn_show_ranks()
    {
        this.carrot.play_sound_click();
        this.carrot.game.Show_List_Top_player();
    }

    public void btn_switch_flag()
    {
        this.carrot.play_sound_click();
        this.player_input.btn_change_flag();
    }

    public void play_sound(int index_sound)
    {
        if(this.carrot.get_status_sound()) this.sound[index_sound].Play();
    }

    public void btn_zoom_out()
    {
        this.carrot.play_sound_click();
        this.cam_game.fieldOfView = this.cam_game.fieldOfView - 5f;
    }

    public void btn_zoom_in()
    {
        this.carrot.play_sound_click();
        this.cam_game.fieldOfView = this.cam_game.fieldOfView + 5f;
    }
}

[Serializable]
public class GameSettings
{
    public static readonly GameSettings Beginner_portrait = new GameSettings(8, 10, 10, "beginner");
    public static readonly GameSettings Intermediate_portrait = new GameSettings(12, 15, 30, "intermediate");
    public static readonly GameSettings Expert_portrait = new GameSettings(16,20, 68, "expert");

    public static readonly GameSettings Beginner_landspace = new GameSettings(10, 8, 10, "beginner");
    public static readonly GameSettings Intermediater_landspace = new GameSettings(17, 15, 40, "intermediate");
    public static readonly GameSettings Expertr_landspace = new GameSettings(20, 16, 99, "expert");

    [SerializeField] private int _height;
    [SerializeField] private int _width;
    [SerializeField] private int _mines;
    [SerializeField] private string _name;

    public int Height
    {
        get { return _height; }
    }

    public int Width
    {
        get { return _width; }
    }

    public int Mines
    {
        get { return _mines; }
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }


    public GameSettings(int w, int h, int m, string s)
    {
        _width = w;
        _height = h;
        _mines = m;
        _name = s;
    }

    public GameSettings()
    {
    }

    public void Set(int w, int h, int m)
    {
        _width = w;
        _height = h;
        _mines = m;
    }

    public bool isValid()
    {
        if ((_width <= 0 || _height <= 0 || _mines <= 0 ) ||(_mines >= _width*_height) ||(_height > 24 || _width > 35) )
            return false;
        return true;
    }


}

