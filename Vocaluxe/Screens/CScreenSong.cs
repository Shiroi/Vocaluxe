﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Vocaluxe.Base;
using Vocaluxe.Lib.Draw;
using Vocaluxe.Lib.Song;
using Vocaluxe.Menu;
using Vocaluxe.Menu.SongMenu;

namespace Vocaluxe.Screens
{
    class CScreenSong : CMenu
    {
        // Version number for theme files. Increment it, if you've changed something on the theme files!
        const int ScreenVersion = 3;

        private const string TextCategory = "TextCategory";
        private const string TextSelection = "TextSelection";
        private const string TextSearchBarTitle = "TextSearchBarTitle";
        private const string TextSearchBar = "TextSearchBar";
        private const string TextOptionsTitle = "TextOptionsTitle";

        private const string ButtonOptionsSing = "ButtonOptionsSing";
        private const string ButtonOptionsPlaylist = "ButtonOptionsPlaylist";
        private const string ButtonOptionsClose = "ButtonOptionsClose";

        private const string SelectSlideOptionsMode = "SelectSlideOptionsMode";

        private const string StaticSearchBar = "StaticSearchBar";
        private const string StaticOptionsBG = "StaticOptionsBG";
        private const string SongMenu = "SongMenu";

        private string _SearchText = String.Empty;
        private bool _SearchActive = false;

        private bool _SongOptionsActive = false;
        private List<GameModes.EGameMode> _AvailableGameModes;

        public CScreenSong()
        {
            Init();

            _AvailableGameModes = new List<GameModes.EGameMode>();
        }

        protected override void Init()
        {
            base.Init();

            _ThemeName = "ScreenSong";
            _ScreenVersion = ScreenVersion;
            _ThemeStatics = new string[] { StaticSearchBar, StaticOptionsBG };
            _ThemeTexts = new string[] { TextCategory, TextSelection, TextSearchBarTitle, TextSearchBar, TextOptionsTitle };
            _ThemeButtons = new string[] { ButtonOptionsClose, ButtonOptionsPlaylist, ButtonOptionsSing };
            _ThemeSelectSlides = new string[] { SelectSlideOptionsMode };
            _ThemeSongMenus = new string[] { SongMenu };
        }

        public override void LoadTheme()
        {
            base.LoadTheme();
            SelectSlides[htSelectSlides(SelectSlideOptionsMode)].Visible = false;
            Buttons[htButtons(ButtonOptionsClose)].Visible = false;
            Buttons[htButtons(ButtonOptionsSing)].Visible = false;
            Buttons[htButtons(ButtonOptionsPlaylist)].Visible = false;
            Texts[htTexts(TextOptionsTitle)].Visible = false;
            Statics[htStatics(StaticOptionsBG)].Visible = false;
        }

        public override bool HandleInput(KeyEvent KeyEvent)
        {
            base.HandleInput(KeyEvent);

            if (!_SongOptionsActive)
            {

                if (KeyEvent.KeyPressed && !Char.IsControl(KeyEvent.Unicode))
                {
                    if (_SearchActive)
                        ApplyNewSearchFilter(_SearchText + KeyEvent.Unicode);
                    /*
                    else if (!Char.IsControl(KeyEvent.Unicode))
                    {
                        JumpTo(KeyEvent.Unicode);
                        return true;
                    } */
                }
                else
                {
                    SongMenus[htSongMenus(SongMenu)].HandleInput(ref KeyEvent);

                    if (KeyEvent.Handled)
                        return true;

                    switch (KeyEvent.Key)
                    {
                        case Keys.Escape:
                            if (CSongs.Category < 0 || CConfig.Tabs == EOffOn.TR_CONFIG_OFF)
                                CGraphics.FadeTo(EScreens.ScreenMain);
                            break;

                        case Keys.Enter:
                            if (CSongs.NumVisibleSongs > 0)
                            {
                                if (SongMenus[htSongMenus(SongMenu)].GetSelectedSong() != -1 && !_SongOptionsActive)
                                    ToggleSongOptions();
                            }
                            break;

                        case Keys.Back:
                            if (_SearchText.Length > 0)
                            {
                                ApplyNewSearchFilter(_SearchText.Remove(_SearchText.Length - 1));
                            }

                            if (!_SearchActive && CSongs.Category < 0)
                                CGraphics.FadeTo(EScreens.ScreenMain);

                            break;

                        case Keys.F3:
                            if (_SearchActive)
                            {
                                _SearchActive = false;
                                _SearchText = String.Empty;
                                ApplyNewSearchFilter(_SearchText);
                            }
                            else
                            {
                                _SearchActive = true;
                            }
                            break;

                        case Keys.A:
                            if (!_SearchActive && KeyEvent.Mod == Modifier.None)
                            {
                                StartRandomAllSongs();
                            }
                            if (KeyEvent.Mod == Modifier.Ctrl)
                            {
                                StartRandomVisibleSongs();
                            }
                            break;

                        case Keys.F:
                            if (KeyEvent.Mod == Modifier.Ctrl)
                            {
                                if (_SearchActive)
                                {
                                    _SearchActive = false;
                                    _SearchText = String.Empty;
                                    ApplyNewSearchFilter(_SearchText);
                                }
                                else
                                {
                                    _SearchActive = true;
                                }
                            }
                            break;

                        case Keys.R:
                            if (CSongs.Category != -1 && KeyEvent.Mod == Modifier.Ctrl)
                            {
                                SongMenus[htSongMenus(SongMenu)].SetSelectedSong(CSongs.GetRandomSong());
                            }
                            break;

                        case Keys.S:
                            if (CSongs.NumVisibleSongs > 0)
                                StartMedleySong(SongMenus[htSongMenus(SongMenu)].GetSelectedSong());
                            break;
                    }
                }
            }

            else
            {
                switch (KeyEvent.Key)
                {
                    case Keys.Enter:
                        if (Buttons[htButtons(ButtonOptionsClose)].Selected)
                            ToggleSongOptions();
                        else if (Buttons[htButtons(ButtonOptionsSing)].Selected)
                        {
                            ToggleSongOptions();
                            StartSong(SongMenus[htSongMenus(SongMenu)].GetSelectedSong());
                        }
                        else if (Buttons[htButtons(ButtonOptionsPlaylist)].Selected)
                            ToggleSongOptions();
                        break;

                    case Keys.Escape:
                    case Keys.Back:
                        ToggleSongOptions();
                        break;
                }
            }

            return true;
        }

        public override bool HandleMouse(MouseEvent MouseEvent)
        {
            base.HandleMouse(MouseEvent);

            if (!_SongOptionsActive)
            {

                if ((MouseEvent.RB) && (CSongs.Category < 0))
                {
                    CGraphics.FadeTo(EScreens.ScreenMain);
                }
                else if (MouseEvent.RB && _SongOptionsActive)
                    ToggleSongOptions();

                if (MouseEvent.MB && CSongs.Category != -1)
                {
                    Console.WriteLine("MB pressed");
                    SongMenus[htSongMenus(SongMenu)].SetSelectedSong(CSongs.GetRandomSong());
                }
                else
                    SongMenus[htSongMenus(SongMenu)].HandleMouse(ref MouseEvent);

                if (MouseEvent.LB && CSongs.NumVisibleSongs > 0 && SongMenus[htSongMenus(SongMenu)].GetActualSelection() != -1)
                {
                    if (SongMenus[htSongMenus(SongMenu)].GetSelectedSong() != -1 && !_SongOptionsActive)
                    {
                        ToggleSongOptions();
                    }
                }
            }

            else
            {
                if (MouseEvent.LB && IsMouseOver(MouseEvent))
                {
                    if (Buttons[htButtons(ButtonOptionsClose)].Selected)
                    {
                        ToggleSongOptions();
                    }
                    else if (Buttons[htButtons(ButtonOptionsSing)].Selected)
                    {
                        ToggleSongOptions();
                        StartSong(SongMenus[htSongMenus(SongMenu)].GetSelectedSong());
                    }
                    else if (Buttons[htButtons(ButtonOptionsPlaylist)].Selected)
                    {
                        ToggleSongOptions();
                    }
                }
                if (MouseEvent.RB)
                {
                    ToggleSongOptions();
                }
            }

            return true;
        }

        public override void OnShow()
        {
            base.OnShow();

            SongMenus[htSongMenus(SongMenu)].OnShow();
        }

        public override bool UpdateGame()
        {
            SongMenus[htSongMenus(SongMenu)].Update();
            Texts[htTexts(TextCategory)].Text = CSongs.GetActualCategoryName();

            if (CSongs.Category > -1 || CConfig.Tabs == EOffOn.TR_CONFIG_OFF)
                CBackgroundMusic.Disabled = true;
            else
                CBackgroundMusic.Disabled = false;

            int song = SongMenus[htSongMenus(SongMenu)].GetActualSelection();
            if ((CSongs.Category >= 0 || CConfig.Tabs == EOffOn.TR_CONFIG_OFF) && song >= 0 && song < CSongs.VisibleSongs.Length)
            {
                Texts[htTexts(TextSelection)].Text = CSongs.VisibleSongs[song].Artist + " - " + CSongs.VisibleSongs[song].Title;                
            }
            else if (CSongs.Category == -1 && song >= 0 && song < CSongs.Categories.Length)
            {
                Texts[htTexts(TextSelection)].Text = CSongs.Categories[song].Name;                
            }
            else
                Texts[htTexts(TextSelection)].Text = String.Empty;

            Texts[htTexts(TextSearchBar)].Text = _SearchText;
            if (_SearchActive)
            {
                Texts[htTexts(TextSearchBar)].Text += '|';

                Texts[htTexts(TextSearchBar)].Visible = true;
                Texts[htTexts(TextSearchBarTitle)].Visible = true;
                Statics[htStatics(StaticSearchBar)].Visible = true;
            }
            else
            {
                Texts[htTexts(TextSearchBar)].Visible = false;
                Texts[htTexts(TextSearchBarTitle)].Visible = false;
                Statics[htStatics(StaticSearchBar)].Visible = false;
            }

            return true;
        }

        public override bool Draw()
        {
            base.Draw();

            if (_SongOptionsActive)
            {
                Statics[htStatics(StaticOptionsBG)].Draw();
                Texts[htTexts(TextOptionsTitle)].Draw();
                SelectSlides[htSelectSlides(SelectSlideOptionsMode)].Draw();
                Buttons[htButtons(ButtonOptionsClose)].Draw();
                Buttons[htButtons(ButtonOptionsSing)].Draw();
                Buttons[htButtons(ButtonOptionsPlaylist)].Draw();
                
            }

            return true;
        }

        public override void OnClose()
        {
            base.OnClose();
            CBackgroundMusic.Disabled = false;
            SongMenus[htSongMenus(SongMenu)].OnHide();
        }

        private void StartSong(int SongNr)
        {
            if ((CSongs.Category >= 0) && (SongNr >= 0))
            {
                if (_AvailableGameModes.Count >= SelectSlides[htSelectSlides(SelectSlideOptionsMode)].Selection)
                {
                    CGame.SetGameMode(_AvailableGameModes[SelectSlides[htSelectSlides(SelectSlideOptionsMode)].Selection]);
                }
                else
                {
                    if (CSongs.VisibleSongs[SongNr].IsDuet)
                        CGame.SetGameMode(GameModes.EGameMode.TR_GAMEMODE_DUET);
                    else
                        CGame.SetGameMode(GameModes.EGameMode.TR_GAMEMODE_NORMAL);
                }

                CGame.Reset();
                CGame.ClearSongs();
                CGame.AddVisibleSong(SongNr);
                //CGame.AddSong(SongNr+1);

                CGraphics.FadeTo(EScreens.ScreenNames);
            }
        }

        private void StartMedleySong(int SongNr)
        {
            if ((CSongs.Category >= 0) && (SongNr >= 0))
            {
                if (CSongs.VisibleSongs[SongNr].Medley.Source != EMedleySource.None)
                    CGame.SetGameMode(GameModes.EGameMode.TR_GAMEMODE_MEDLEY);
                else
                    return;

                CGame.Reset();
                CGame.ClearSongs();
                CGame.AddVisibleSong(SongNr);

                CGraphics.FadeTo(EScreens.ScreenNames);
            }
        }

        private void StartRandomAllSongs()
        {
            CGame.Reset();
            CGame.ClearSongs();
            CGame.SetGameMode(GameModes.EGameMode.TR_GAMEMODE_NORMAL);

            List<int> IDs = new List<int>();
            for (int i = 0; i < CSongs.AllSongs.Length; i++)
            {
                IDs.Add(i);
            }

            while (IDs.Count > 0)
            {
                int SongNr = IDs[CGame.Rand.Next(IDs.Count)];

                if (!CSongs.AllSongs[SongNr].IsDuet)
                {
                    CGame.AddSong(SongNr);
                }
                IDs.Remove(SongNr);    
            }

            if (CGame.GetNumSongs() > 0)
                CGraphics.FadeTo(EScreens.ScreenNames);
        }

        private void StartRandomVisibleSongs()
        {
            CGame.Reset();
            CGame.ClearSongs();
            CGame.SetGameMode(GameModes.EGameMode.TR_GAMEMODE_NORMAL);

            List<int> IDs = new List<int>();
            for (int i = 0; i < CSongs.VisibleSongs.Length; i++)
            {
                IDs.Add(CSongs.VisibleSongs[i].ID);
            }

            while (IDs.Count > 0)
            {
                int SongNr = IDs[CGame.Rand.Next(IDs.Count)];

                if (!CSongs.AllSongs[SongNr].IsDuet)
                {
                    CGame.AddSong(SongNr);
                }
                IDs.Remove(SongNr);
            }

            if (CGame.GetNumSongs() > 0)
                CGraphics.FadeTo(EScreens.ScreenNames);
        }

        private void JumpTo(char Letter)
        {
            int song = SongMenus[htSongMenus(SongMenu)].GetSelectedSong();
            int id = -1;
            if (song > -1 && song < CSongs.NumVisibleSongs)
            {
                id = CSongs.VisibleSongs[song].ID;
            }

            int visibleID = Array.FindIndex<Vocaluxe.Lib.Song.CSong>(CSongs.VisibleSongs, element => element.Artist.StartsWith(Letter.ToString(), StringComparison.OrdinalIgnoreCase));
            if (visibleID > -1)
            {
                id = visibleID;
                SongMenus[htSongMenus(SongMenu)].SetSelectedSong(id);
            }
        }

        private void ApplyNewSearchFilter(string NewFilterString)
        {
            int song = SongMenus[htSongMenus(SongMenu)].GetSelectedSong();
            int id = -1;
            if (song > -1 && song < CSongs.NumVisibleSongs)
            {
                id = CSongs.VisibleSongs[song].ID;
            }

            _SearchText = NewFilterString;
            CSongs.SearchFilter = _SearchText;

            if (NewFilterString != String.Empty)
                CSongs.Category = 0;
            else
                CSongs.Category = -1;

            if (id > -1)
            {
                if (CSongs.NumVisibleSongs > 0)
                {
                    if (id != CSongs.VisibleSongs[0].ID)
                        SongMenus[htSongMenus(SongMenu)].OnHide();
                }
            }

            if (CSongs.NumVisibleSongs == 0)
                SongMenus[htSongMenus(SongMenu)].OnHide();

            SongMenus[htSongMenus(SongMenu)].OnShow();
        }

        private void ToggleSongOptions()
        {
            _SongOptionsActive = !_SongOptionsActive;
            if (_SongOptionsActive)
            {
                SetInteractionToButton(Buttons[htButtons(ButtonOptionsSing)]);
                _AvailableGameModes.Clear();
                SelectSlides[htSelectSlides(SelectSlideOptionsMode)].Clear();
                if (!CSongs.VisibleSongs[SongMenus[htSongMenus(SongMenu)].GetSelectedSong()].IsDuet)
                {
                    SelectSlides[htSelectSlides(SelectSlideOptionsMode)].AddValue(Enum.GetName(typeof(GameModes.EGameMode), GameModes.EGameMode.TR_GAMEMODE_NORMAL));
                    _AvailableGameModes.Add(GameModes.EGameMode.TR_GAMEMODE_NORMAL);
                    SelectSlides[htSelectSlides(SelectSlideOptionsMode)].AddValue(Enum.GetName(typeof(GameModes.EGameMode), GameModes.EGameMode.TR_GAMEMODE_SHORTSONG));
                    _AvailableGameModes.Add(GameModes.EGameMode.TR_GAMEMODE_SHORTSONG);                
                }
                if (CSongs.VisibleSongs[SongMenus[htSongMenus(SongMenu)].GetSelectedSong()].IsDuet)
                {
                    SelectSlides[htSelectSlides(SelectSlideOptionsMode)].AddValue(Enum.GetName(typeof(GameModes.EGameMode), GameModes.EGameMode.TR_GAMEMODE_DUET));
                    _AvailableGameModes.Add(GameModes.EGameMode.TR_GAMEMODE_DUET);
                }
                if (CSongs.VisibleSongs[SongMenus[htSongMenus(SongMenu)].GetSelectedSong()].Medley.Source != EMedleySource.None)
                {
                    SelectSlides[htSelectSlides(SelectSlideOptionsMode)].AddValue(Enum.GetName(typeof(GameModes.EGameMode), GameModes.EGameMode.TR_GAMEMODE_MEDLEY));
                    _AvailableGameModes.Add(GameModes.EGameMode.TR_GAMEMODE_MEDLEY);
                }
                SelectSlides[htSelectSlides(SelectSlideOptionsMode)].Visible = true;
                Buttons[htButtons(ButtonOptionsClose)].Visible = true;
                Buttons[htButtons(ButtonOptionsSing)].Visible = true;
                Buttons[htButtons(ButtonOptionsPlaylist)].Visible = true;
                Texts[htTexts(TextOptionsTitle)].Visible = true;
                Statics[htStatics(StaticOptionsBG)].Visible = true;
            }
            else 
            {
                SelectSlides[htSelectSlides(SelectSlideOptionsMode)].Visible = false;
                Buttons[htButtons(ButtonOptionsClose)].Visible = false;
                Buttons[htButtons(ButtonOptionsSing)].Visible = false;
                Buttons[htButtons(ButtonOptionsPlaylist)].Visible = false;
                Texts[htTexts(TextOptionsTitle)].Visible = false;
                Statics[htStatics(StaticOptionsBG)].Visible = false;
            }
        }
    }
}
