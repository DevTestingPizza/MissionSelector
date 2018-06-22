using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace MissionSelector
{
    public class Main : BaseScript
    {
        // Variables
        struct Card
        {
            public int JobType { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public float MoneyMultiplier { get; set; }
            public float RPMultiplier { get; set; }
            public int Votes { get; set; }
            public int VoteColor { get; set; }
            public bool Selected { get; set; }
            public int Rating { get; set; }
            public string CreatedBy { get; set; }
            public int MinPlayers { get; set; }
            public int MaxPlayers { get; set; }
            public string AreaLabel { get; set; }
            public string TextureDict { get; set; }
            public string TextureName { get; set; }

            public Card(int JobType, string Title, string Description, float MoneyMultiplier, float RPMultiplier, int Votes, int VoteColor, bool Selected, int Rating, string CreatedBy, int MinPlayers, int MaxPlayers, string AreaLabel, string TextureDict, string TextureName)
            {
                this.JobType = JobType;
                this.Title = Title;
                this.Description = Description;
                this.MoneyMultiplier = MoneyMultiplier;
                this.RPMultiplier = RPMultiplier;
                this.Votes = Votes;
                this.VoteColor = VoteColor;
                this.Selected = Selected;
                this.Rating = Rating;
                this.CreatedBy = CreatedBy;
                this.MinPlayers = MinPlayers;
                this.MaxPlayers = MaxPlayers;
                this.AreaLabel = AreaLabel;
                this.TextureDict = TextureDict;
                this.TextureName = TextureName;
            }

        }
        private Card[] Cards = new Card[6];
        private List<int> Buttons = new List<int>() { 0, 0, 0 };
        private int SelectedButton = -1;

        private bool firstTick = true;
        private Scaleform SelectorScaleform;
        private Scaleform InstructionalButtonsScaleform = new Scaleform("INSTRUCTIONAL_BUTTONS");

        private int currentSelection = 0;
        private bool SelectorVisible = false;
        private bool PlayerListVisible = false;
        private int timer = GetGameTimer();

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {

            Tick += OnTick;
        }


        /// <summary>
        /// OnTick task to display, load and setup the scaleform as well as managing all controlls and actions.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            if (Game.IsControlJustPressed(0, Control.FrontendSocialClub) && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
            {
                ToggleSelector();
            }

            if (firstTick)
            {

                RequestStreamedTextureDict("cardimages", true);
                while (!HasStreamedTextureDictLoaded("cardimages"))
                {
                    await Delay(0);
                }

                Cards[0] = new Card(0, "Mission", "This is a simple description. Descriptions can be very, very, VERY long. Or they can be very short. It doesn't matter, nobody reads them anyway.", 0f, 0f, 0, 18, false, 100, "Vespura", 1, 4, "AIRP", "cardimages", "1");
                Cards[1] = new Card(1, "Team Deathmatch", "Description?", 0f, 0f, 0, 18, false, 99, "Vespura", 1, 4, "AIRP", "cardimages", "2");
                Cards[2] = new Card(2, "Deathmatch", "THAT WAS A HEADSHOT, YOU MOTHER FUCKER!", 0f, 0f, 0, 18, false, 100, "Vespura", 1, 4, "AIRP", "cardimages", "3");
                Cards[3] = new Card(3, "Super Fun Race", "I'm bored already.", 0f, 0f, 0, 18, false, 100, "Vespura", 1, 4, "AIRP", "cardimages", "4");
                Cards[4] = new Card(4, "Die Already (Survival)", "Please just die.", 0f, 0f, 0, 18, false, 100, "Vespura", 1, 4, "AIRP", "cardimages", "5");
                Cards[5] = new Card(0, "Some Mission", "This is a another simple description.", 0f, 0f, 0, 18, false, 100, "Vespura", 1, 4, "AIRP", "cardimages", "6");

                firstTick = false;

                SelectorScaleform = new Scaleform("MP_NEXT_JOB_SELECTION");
                while (!SelectorScaleform.IsLoaded)
                {
                    await Delay(0);
                }

                SetTitle(SelectorScaleform.Handle, votes: 3, totalVotes: 5);

                for (var i = 0; i < 6; i++)
                {
                    SetGridItem(SelectorScaleform.Handle, i, Cards[i].Title, Cards[i].JobType, 0, false, Cards[i].RPMultiplier, Cards[i].MoneyMultiplier, false, 9, Cards[i].TextureDict, Cards[i].TextureName);
                }

                SetGridButtons(SelectorScaleform.Handle, 3);

                SetSelection(SelectorScaleform.Handle, currentSelection, Cards[currentSelection].Title, Cards[currentSelection].Description);

            }
            else if (SelectorVisible)
            {
                if (Game.IsControlJustPressed(0, (Control)204))
                {
                    Game.PlaySound("SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
                    UpdatePlayerlist(true);
                    timer = GetGameTimer();
                }

                if (PlayerListVisible)
                {
                    if (GetGameTimer() - timer > 8000)
                    {
                        UpdatePlayerlist(false);
                        timer = GetGameTimer();
                    }
                }

                if (!PlayerListVisible)
                {
                    if (Game.IsControlJustPressed(0, Control.FrontendLeft))
                    {
                        GoLeft(true);
                    }
                    if (Game.IsControlJustPressed(0, Control.FrontendRight))
                    {
                        GoRight(true);
                    }
                    if (Game.IsControlJustPressed(0, Control.FrontendUp))
                    {
                        GoUp(true);
                    }
                    if (Game.IsControlJustPressed(0, Control.FrontendDown))
                    {
                        GoDown(true);
                    }
                    if (Game.IsControlJustPressed(0, Control.FrontendAccept))
                    {
                        SelectItem();
                    }
                }


                HideHudAndRadarThisFrame();
                SelectorScaleform.Render2D();
                for (int i = 0; i < 20; i++)
                {
                    InstructionalButtonsScaleform.CallFunction("SET_DATA_SLOT_EMPTY", i);
                }
                if (PlayerListVisible)
                {
                    InstructionalButtonsScaleform.CallFunction("SET_DATA_SLOT_EMPTY", 0);
                    InstructionalButtonsScaleform.CallFunction("SET_DATA_SLOT_EMPTY", 1);
                    InstructionalButtonsScaleform.CallFunction("SET_DATA_SLOT", 0, GetControlInstructionalButton(0, 204, 0), GetLabelText("FMMC_END_LST"));
                }
                else
                {
                    InstructionalButtonsScaleform.CallFunction("SET_DATA_SLOT", 0, GetControlInstructionalButton(0, 201, 0), GetLabelText("FMMC_END_VOT"));
                    InstructionalButtonsScaleform.CallFunction("SET_DATA_SLOT", 1, GetControlInstructionalButton(0, 204, 0), GetLabelText("FMMC_END_LST"));
                }


                InstructionalButtonsScaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
                InstructionalButtonsScaleform.Render2D();
            }
        }


        /// <summary>
        /// Selects the currently hovered item.
        /// </summary>
        private void SelectItem()
        {
            if (currentSelection < 6)
            {
                if (!Cards[currentSelection].Selected)
                {
                    ShowPlayerVote(SelectorScaleform.Handle, currentSelection, Game.Player.Name, 255, 255, 255);

                    for (var i = 0; i < 6; i++)
                    {
                        if (Cards[i].Selected)
                        {
                            Cards[i].Selected = false;
                            Cards[i].Votes--;
                            SetGridItemVote(SelectorScaleform.Handle, i, Cards[i].Votes, Cards[i].VoteColor, false, true);
                            break;
                        }
                    }
                    if (SelectedButton != -1)
                    {
                        Buttons[SelectedButton]--;
                        SetGridItemVote(SelectorScaleform.Handle, SelectedButton + 6, Buttons[SelectedButton], 18, false, true);
                        SelectedButton = -1;
                    }
                    Cards[currentSelection].Selected = true;
                    Cards[currentSelection].Votes++;
                    SetGridItemVote(SelectorScaleform.Handle, currentSelection, Cards[currentSelection].Votes, Cards[currentSelection].VoteColor, true, true);
                }
            }
            else
            {
                for (var i = 0; i < 6; i++)
                {
                    if (Cards[i].Selected)
                    {
                        Cards[i].Selected = false;
                        Cards[i].Votes--;

                        SetGridItemVote(SelectorScaleform.Handle, i, Cards[i].Votes, Cards[i].VoteColor, false, true);
                        break;
                    }
                }
                if (SelectedButton != -1)
                {
                    Buttons[SelectedButton]--;
                    SetGridItemVote(SelectorScaleform.Handle, SelectedButton + 6, Buttons[SelectedButton], 18, false, true);
                }
                SelectedButton = currentSelection - 6;
                Buttons[SelectedButton]++;
                SetGridItemVote(SelectorScaleform.Handle, currentSelection, Buttons[SelectedButton], 18, true, true);
            }
        }


        /// <summary>
        /// Updates the playerlist and checks for new players.
        /// </summary>
        /// <param name="toggleState"></param>
        private async void UpdatePlayerlist(bool toggleState)
        {
            int iterator = 0;
            if (!IsPedheadshotValid(1))
            {
                for (int x = 0; x < 132; x++)
                {
                    UnregisterPedheadshot(x);
                }
            }
            foreach (Player p in new PlayerList())
            {
                var tmp = RegisterPedheadshot(GetPlayerPed(p.Handle));
                while (!IsPedheadshotReady(tmp))
                {
                    await Delay(0);
                }
                string headshot = "";
                if (IsPedheadshotValid(tmp))
                {
                    headshot = GetPedheadshotTxdString(tmp);
                }
                SelectorScaleform.CallFunction("SET_LOBBY_LIST_DATA_SLOT_EMPTY", iterator);
                SelectorScaleform.CallFunction("SET_LOBBY_LIST_DATA_SLOT", iterator, 0, 0, 2, p.ServerId, true, p.Name, 116, false, 0, 65, 0, "", 0, "", 18, headshot, headshot);

                iterator++;
            }

            if (toggleState)
            {
                PlayerListVisible = !PlayerListVisible;

                PushScaleformMovieFunction(SelectorScaleform.Handle, "SET_LOBBY_LIST_VISIBILITY");
                PushScaleformMovieMethodParameterBool(PlayerListVisible);
                EndScaleformMovieMethod();
            }


            if (PlayerListVisible)
            {
                PushScaleformMovieFunction(SelectorScaleform.Handle, "DISPLAY_LOBBY_LIST_VIEW");
                EndScaleformMovieMethod();
            }

            PushScaleformMovieFunction(SelectorScaleform.Handle, "SET_ITEMS_GREYED_OUT");
            PushScaleformMovieMethodParameterBool(PlayerListVisible);
            EndScaleformMovieMethod();

            int iter = 0;
            foreach (Player p in new PlayerList())
            {
                if (p.Handle == Game.Player.Handle)
                {
                    PushScaleformMovieFunction(SelectorScaleform.Handle, "SET_LOBBY_LIST_HIGHLIGHT");
                    PushScaleformMovieMethodParameterInt(iter);
                    EndScaleformMovieMethod();
                    break;
                }
                iter++;
            }
        }


        /// <summary>
        /// Toggle the selector scaleform visible/invisible.
        /// </summary>
        private void ToggleSelector()
        {
            SelectorVisible = !SelectorVisible;
            if (!SelectorVisible)
            {
                TransitionFromBlurred(1f);
                StopAudioScene("MP_LEADERBOARD_SCENE");
                SetAudioFlag("PlayMenuMusic", false);
            }
            else
            {
                TransitionToBlurred(1f);
                StartAudioScene("MP_LEADERBOARD_SCENE");
                SetAudioFlag("PlayMenuMusic", true);
            }
        }


        #region Move selector functions
        /// <summary>
        /// Move left (optionally overflowing to the right)
        /// </summary>
        /// <param name="overflow"></param>
        private void GoLeft(bool overflow)
        {
            int oldSelection = currentSelection;
            if (currentSelection == 0)
            {
                currentSelection = overflow ? 2 : 0;
            }
            else if (currentSelection == 3)
            {
                currentSelection = overflow ? 5 : 3;
            }
            else if (currentSelection == 6)
            {
                currentSelection = overflow ? 8 : 6;
            }
            else
            {
                currentSelection--;
            }
            if (currentSelection < 6)
            {
                SetSelection(SelectorScaleform.Handle, currentSelection, Cards[currentSelection].Title, Cards[currentSelection].Description);
            }
            else
            {
                SetSelection(SelectorScaleform.Handle, currentSelection);
            }

            if (currentSelection != oldSelection || overflow)
            {
                Game.PlaySound("NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
            }
            //SetDescriptions();
        }

        /// <summary>
        /// Move right (optionally overflowing to the left)
        /// </summary>
        /// <param name="overflow"></param>
        private void GoRight(bool overflow)
        {
            int oldSelection = currentSelection;
            if (currentSelection == 2)
            {
                currentSelection = overflow ? 0 : 2;
            }
            else if (currentSelection == 5)
            {
                currentSelection = overflow ? 3 : 5;
            }
            else if (currentSelection == 8)
            {
                currentSelection = overflow ? 6 : 8;
            }
            else
            {
                currentSelection++;
            }
            if (currentSelection < 6)
            {
                SetSelection(SelectorScaleform.Handle, currentSelection, Cards[currentSelection].Title, Cards[currentSelection].Description);
            }
            else
            {
                SetSelection(SelectorScaleform.Handle, currentSelection);
            }
            if (currentSelection != oldSelection || overflow)
            {
                Game.PlaySound("NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
            }
            //SetDescriptions();
        }

        /// <summary>
        /// Move up (optionally overflowing to the bottom)
        /// </summary>
        /// <param name="overflow"></param>
        private void GoUp(bool overflow)
        {
            int oldSelection = currentSelection;
            currentSelection -= 3;
            if (currentSelection < 0)
            {
                currentSelection = overflow ? currentSelection + 9 : oldSelection;
            }
            if (currentSelection < 6)
            {
                SetSelection(SelectorScaleform.Handle, currentSelection, Cards[currentSelection].Title, Cards[currentSelection].Description);
            }
            else
            {
                SetSelection(SelectorScaleform.Handle, currentSelection);
            }
            if (currentSelection != oldSelection || overflow)
            {
                Game.PlaySound("NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
            }
            //SetDescriptions();
        }

        /// <summary>
        /// Move down (optionally overflowing to the top)
        /// </summary>
        /// <param name="overflow"></param>
        private void GoDown(bool overflow)
        {
            int oldSelection = currentSelection;
            currentSelection += 3;
            if (currentSelection > 8)
            {
                currentSelection = overflow ? currentSelection - 9 : oldSelection;
            }
            if (currentSelection < 6)
            {
                SetSelection(SelectorScaleform.Handle, currentSelection, Cards[currentSelection].Title, Cards[currentSelection].Description);
            }
            else
            {
                SetSelection(SelectorScaleform.Handle, currentSelection);
            }
            if (currentSelection != oldSelection || overflow)
            {
                Game.PlaySound("NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
            }
            //SetDescriptions();
        }
        #endregion

        #region scaleform configuration functions
        /// <summary>
        /// Set the title of the scaleform.
        /// </summary>
        /// <param name="scaleformHandle">The handle of the scaleform to change.</param>
        /// <param name="titleLabel">The title label to set on the scaleform.</param>
        /// <param name="votes">The current votes.</param>
        /// <param name="totalVotes">The total votes.</param>
        private void SetTitle(int scaleformHandle, string titleLabel = "FM_NXT_J_TIT", int votes = 0, int totalVotes = 1)
        {
            PushScaleformMovieFunction(scaleformHandle, "SET_TITLE");

            // Set the title using the label.
            BeginTextCommandScaleformString(titleLabel);    // title label
            EndTextCommandScaleformString();

            // Set the votes on the right.
            BeginTextCommandScaleformString("FM_NXT_VCNT");
            AddTextComponentInteger(votes);         // current votes
            AddTextComponentInteger(totalVotes);    // max votes
            EndTextCommandScaleformString();
            EndScaleformMovieMethod();
        }

        /// <summary>
        /// Animation to show whenever a player votes for a grid item.
        /// </summary>
        /// <param name="scaleformHandle">Scaleform handle to change.</param>
        /// <param name="index">Index of the selected card.</param>
        /// <param name="playerName">Name of the player that voted.</param>
        /// <param name="r">Player name RGB red.</param>
        /// <param name="g">Player name RGB green.</param>
        /// <param name="b">Player name RGB blue.</param>
        private void ShowPlayerVote(int scaleformHandle, int index, string playerName, int r, int g, int b)
        {
            PushScaleformMovieFunction(scaleformHandle, "SHOW_PLAYER_VOTE");
            PushScaleformMovieMethodParameterInt(index);      // grid item
            N_0xe83a3e3557a56640(playerName);                   // _PUSH_SCALEFORM_MOVIE_METHOD_PARAMETER_BUTTON_NAME
            PushScaleformMovieMethodParameterInt(r);          // red
            PushScaleformMovieMethodParameterInt(g);          // green
            PushScaleformMovieMethodParameterInt(b);          // blue
            EndScaleformMovieMethod();
        }

        /// <summary>
        /// Set a grid item.
        /// </summary>
        /// <param name="scaleformHandle">Scaleform handle to use.</param>
        /// <param name="itemIndex">Grid item index.</param>
        /// <param name="itemText">Grid item title text.</param>
        /// <param name="rockstarLogoType">Logo type 0 = NONE, 1 = VERIFIED BY R*, 2 = CREATED BY R*.</param>
        /// <param name="jobType">Mission/Icon type: -1 = NONE, 0 = MISSION, 1 = DEATHMATCH, 2 = RACE, 3 = SURVIVAL, 4 = TEAM DEATHMATCH.</param>
        /// <param name="previouslyCompleted">Checkmark to indicate that the player has completed this job before.</param>
        /// <param name="rpMultiplier">RP multiplier value.</param>
        /// <param name="moneyMultiplier">Money multiplier value.</param>
        /// <param name="itemDisabled">Gray out the item/disable it.</param>
        /// <param name="iconColor">(Hud) icon color, common values: 1 (white), 6 (red), 9 (blue) or 18 (green).</param>
        /// <param name="textureDict">Texture dictionary name for the background image.</param>
        /// <param name="textureName">Texture name for the background image.</param>
        private void SetGridItem(int scaleformHandle, int itemIndex, string itemText, int jobType, int rockstarLogoType, bool previouslyCompleted, float rpMultiplier, float moneyMultiplier, bool itemDisabled, int iconColor, string textureDict, string textureName)
        {
            int jobIcon = 0;
            if (jobType == 0)
            {
                jobIcon = 0;
            }
            else if (jobType == 1)
            {
                jobIcon = 1;
            }
            else if (jobType == 2)
            {
                jobIcon = 4;
            }
            else if (jobType == 3)
            {
                jobIcon = 2;
            }
            else if (jobType == 4)
            {
                jobIcon = 3;
            }
            PushScaleformMovieFunction(scaleformHandle, "SET_GRID_ITEM");
            PushScaleformMovieMethodParameterInt(itemIndex);          // Item index.
            PushScaleformMovieMethodParameterString(itemText);        // Item title.
            PushScaleformMovieMethodParameterString(textureDict);     // Item texture dictionary name.
            PushScaleformMovieMethodParameterString(textureName);     // Item texture name.
            PushScaleformMovieMethodParameterInt(1);                  // Item background: -1 no background, 0 blank background, 1 texture background.
            PushScaleformMovieMethodParameterInt(rockstarLogoType);   // Item logo type: 0 = No logo, 1 = R* Verified logo, 2+ = R* Created logo.
            PushScaleformMovieMethodParameterInt(jobIcon);           // Item icon: -1 = NONE, 0 = MISSION, 1 = DEATHMATCH, 2 = RACE, 3 = SURVIVAL, 4 = TEAM DEATHMATCH.
            PushScaleformMovieMethodParameterBool(previouslyCompleted);   // Job previously completed checkmark.
            PushScaleformMovieMethodParameterFloat(rpMultiplier);         // RP multiplier value.
            PushScaleformMovieMethodParameterFloat(moneyMultiplier);      // Money multiplier value.
            PushScaleformMovieMethodParameterBool(itemDisabled);  // Disabled/grayed out state.
            PushScaleformMovieMethodParameterInt(iconColor);      // Icon (hud) color, common values: 1 (white), 6 (red), 9 (blue) or 18 (green).
            EndScaleformMovieMethod();
        }


        /// <summary>
        /// Set grid item votes.
        /// </summary>
        /// <param name="scaleformHandle">Scaleform handle to use.</param>
        /// <param name="itemIndex">Grid item index.</param>
        /// <param name="votesCount">Number of votes for this grid item.</param>
        /// <param name="hudColor">Hud color for the vote, use: 1 (white), 6 (red) or 18 (green).</param>
        /// <param name="checkmark">Checkmark indicator to show if you have slected this item yourself.</param>
        /// <param name="flashCard">Flash the grid item to indicate a vote count change.</param>
        private void SetGridItemVote(int scaleformHandle, int itemIndex, int votesCount, int hudColor, bool checkmark, bool flashCard = true)
        {
            PushScaleformMovieFunction(scaleformHandle, "SET_GRID_ITEM_VOTE");

            PushScaleformMovieMethodParameterInt(itemIndex);      // the card index
            PushScaleformMovieMethodParameterInt(votesCount);     // amount of votes on this card
            PushScaleformMovieMethodParameterInt(hudColor);       // 18 (green), 6 (red) or 1 (white) in decompiled scripts
            PushScaleformMovieMethodParameterBool(checkmark);     // shows checkmark
            PushScaleformMovieMethodParameterBool(flashCard);     // flash card once.
            EndScaleformMovieMethod();
        }

        /// <summary>
        /// Set the grid buttons including labels with descriptions.
        /// </summary>
        /// <param name="scaleformHandle"></param>
        /// <param name="refreshUsesRemaining"></param>
        private void SetGridButtons(int scaleformHandle, int refreshUsesRemaining)
        {

            // REPLAY (1ST BTN)
            PushScaleformMovieFunction(scaleformHandle, "SET_GRID_ITEM");
            // grid item index
            PushScaleformMovieMethodParameterInt(6);
            // replay label
            BeginTextCommandScaleformString("FM_NXT_TREP");
            EndTextCommandScaleformString();
            // misc item setup
            PushScaleformMovieMethodParameterString("");      // texture dict
            PushScaleformMovieMethodParameterString("");      // texture name
            PushScaleformMovieMethodParameterInt(-1);         // background
            PushScaleformMovieMethodParameterInt(-1);         // logo type
            PushScaleformMovieMethodParameterInt(-1);         // icon
            PushScaleformMovieMethodParameterBool(false);     // already completed
            PushScaleformMovieMethodParameterFloat(-1f);      // rp multiplier
            PushScaleformMovieMethodParameterFloat(-1f);      // money multiplier
            PushScaleformMovieMethodParameterBool(false);     // disabled
            PushScaleformMovieMethodParameterInt(1);          // icon color
                                                              // End func
            EndScaleformMovieMethod();




            // REFRESH <AMOUNT> (2nd BTN)
            PushScaleformMovieFunction(scaleformHandle, "SET_GRID_ITEM");
            // grid item index
            PushScaleformMovieMethodParameterInt(7);
            // refresh label
            BeginTextCommandScaleformString("FM_NXT_TREFN");
            AddTextComponentInteger(refreshUsesRemaining);
            EndTextCommandScaleformString();
            // misc item setup
            PushScaleformMovieMethodParameterString("");      // texture dict
            PushScaleformMovieMethodParameterString("");      // texture name
            PushScaleformMovieMethodParameterInt(-1);         // background
            PushScaleformMovieMethodParameterInt(-1);         // logo type
            PushScaleformMovieMethodParameterInt(-1);         // icon
            PushScaleformMovieMethodParameterBool(false);     // already completed
            PushScaleformMovieMethodParameterFloat(-1f);      // rp multiplier
            PushScaleformMovieMethodParameterFloat(-1f);      // money multiplier
            PushScaleformMovieMethodParameterBool(false);     // disabled
            PushScaleformMovieMethodParameterInt(1);          // icon color
                                                              // End func
            EndScaleformMovieMethod();




            // RANDOM (3rd BTN)
            PushScaleformMovieFunction(scaleformHandle, "SET_GRID_ITEM");
            // grid item index
            PushScaleformMovieMethodParameterInt(8);
            // random label
            BeginTextCommandScaleformString("FMMC_VEH_RAND");
            EndTextCommandScaleformString();
            // misc item setup
            PushScaleformMovieMethodParameterString("");      // texture dict
            PushScaleformMovieMethodParameterString("");      // texture name
            PushScaleformMovieMethodParameterInt(-1);         // background
            PushScaleformMovieMethodParameterInt(-1);         // logo type
            PushScaleformMovieMethodParameterInt(-1);         // icon
            PushScaleformMovieMethodParameterBool(false);     // already completed
            PushScaleformMovieMethodParameterFloat(-1f);      // rp multiplier
            PushScaleformMovieMethodParameterFloat(-1f);      // money multiplier
            PushScaleformMovieMethodParameterBool(false);     // disabled
            PushScaleformMovieMethodParameterInt(1);          // icon color
                                                              // End func
            EndScaleformMovieMethod();

        }

        /// <summary>
        /// Sets the selected index.
        /// </summary>
        /// <param name="scaleformHandle"></param>
        /// <param name="itemIndex"></param>
        private void SetSelection(int scaleformHandle, int itemIndex, string title = "", string description = "")
        {

            PushScaleformMovieFunction(scaleformHandle, "SET_SELECTION");
            PushScaleformMovieMethodParameterInt(itemIndex);

            if (itemIndex == 6)
            {
                BeginTextCommandScaleformString("FM_NXT_SUB1");
                EndTextCommandScaleformString();
                BeginTextCommandScaleformString("FM_NXT_REP");
                EndTextCommandScaleformString();
            }
            else if (itemIndex == 7)
            {
                BeginTextCommandScaleformString("FM_NXT_SUB2");
                EndTextCommandScaleformString();
                BeginTextCommandScaleformString("FM_NXT_REF");
                EndTextCommandScaleformString();
            }
            else if (itemIndex == 8)
            {
                PushScaleformMovieMethodParameterString("Vote for a random Job");
                PushScaleformMovieMethodParameterString("Vote to play a random Job from this page.");
            }
            else
            {
                PushScaleformMovieMethodParameterButtonName(title);
                PushScaleformMovieMethodParameterButtonName(description);
            }

            PushScaleformMovieMethodParameterBool(false);
            EndScaleformMovieMethod();


            if (currentSelection < 6)
            {
                SetDetailsItem(scaleformHandle: SelectorScaleform.Handle, rating: Cards[currentSelection].Rating, createdBy: Cards[currentSelection].CreatedBy, minPlayers: Cards[currentSelection].MinPlayers, maxPlayers: Cards[currentSelection].MaxPlayers, jobType: Cards[currentSelection].JobType, areaLabel: Cards[currentSelection].AreaLabel);
            }

        }

        //private void SetDescriptions()
        //{
        //    //SetDetailsItem(scaleformHandle: SelectorScaleform.Handle, rating: 82, createdBy: "Vespura", minPlayers: 1, maxPlayers: 16, jobType: 2, areaLabel: "MTCHIL");

        //    //    SetDetailsItem(SelectorScaleform.Handle, 0, 0, 0, 1, 0, 1, "FM_NXT_RAT", "82", false);              // rating
        //    //    SetDetailsItem(SelectorScaleform.Handle, 1, 0, 0, 3, 0, 1, "FM_NXT_CRE", "Vespura", true);          // created by
        //    //    SetDetailsItem(SelectorScaleform.Handle, 2, 1, 32, 0, 0, 1, "FM_NXT_PLY", "", false);               // players
        //    //    SetDetailsItem(SelectorScaleform.Handle, 3, 0, 0, 2, 0, 1, "FM_NXT_TYP", "FMMC_MPM_TY0", false);    // type
        //    //    SetDetailsItem(SelectorScaleform.Handle, 4, 0, 0, 0, 0, 1, "FM_NXT_ARA", "MTCHIL", false);          // area
        //}


        /*
         * Job Types:
         * 
         * -1 = None
         *  0 = Mission
         *  1 = Team Deathmatch
         *  2 = Deathmatch
         *  3 = Race
         *  4 = Survival
         *  5 = Idk
         */

        #region Details item config
        /// <summary>
        /// Sets the info for the details/info card in the bottom right.
        /// </summary>
        /// <param name="scaleformHandle">Scaleform handle to use.</param>
        /// <param name="rating">The rating for the selected mission.</param>
        /// <param name="createdBy">The name of the creator of the mission.</param>
        /// <param name="minPlayers">Min required players.</param>
        /// <param name="maxPlayers">Max allowed players.</param>
        /// <param name="jobType">Job type, 0 MISSION, 1 TEAM DEATHMATCH, 2, DEATHMATCH, 3 RACE, 4 SURVIVAL.</param>
        /// <param name="areaLabel">Label of the in-game area where the job is located at.</param>
        private void SetDetailsItem(int scaleformHandle, int rating, string createdBy, int minPlayers, int maxPlayers, int jobType, string areaLabel)
        {
            #region Rating row
            PushScaleformMovieFunction(scaleformHandle, "SET_DETAILS_ITEM");
            PushScaleformMovieMethodParameterInt(0); // row
            PushScaleformMovieMethodParameterInt(0); // unknown 1
            PushScaleformMovieMethodParameterInt(0); // unknown 2
            PushScaleformMovieMethodParameterInt(0); // style/icon
            PushScaleformMovieMethodParameterInt(0); // unknown 3
            PushScaleformMovieMethodParameterInt(0); // unknown 4

            BeginTextCommandScaleformString("FM_NXT_RAT");  // left label
            EndTextCommandScaleformString();

            BeginTextCommandScaleformString("FM_NXT_RAT1"); // rating row setup
            AddTextComponentInteger(rating);
            EndTextCommandScaleformString();
            EndScaleformMovieMethod();
            #endregion


            #region Created by row
            PushScaleformMovieFunction(scaleformHandle, "SET_DETAILS_ITEM");
            PushScaleformMovieMethodParameterInt(1); // row
            PushScaleformMovieMethodParameterInt(0); // unknown 1
            PushScaleformMovieMethodParameterInt(0); // unknown 2
            PushScaleformMovieMethodParameterInt(3); // style/icon
            PushScaleformMovieMethodParameterInt(0); // unknown 3
            PushScaleformMovieMethodParameterInt(0); // unknown 4

            BeginTextCommandScaleformString("FM_NXT_CRE");  // left label
            EndTextCommandScaleformString();

            PushScaleformMovieMethodParameterButtonName(createdBy); // playername

            BeginTextCommandScaleformString("");                    // unused
            EndTextCommandScaleformString();

            PushScaleformMovieMethodParameterBool(true);            // rockstar social club logo visible

            EndScaleformMovieMethod();
            #endregion


            #region Players row
            PushScaleformMovieFunction(scaleformHandle, "SET_DETAILS_ITEM");
            PushScaleformMovieMethodParameterInt(2); // row
            PushScaleformMovieMethodParameterInt(0); // unknown 1
            PushScaleformMovieMethodParameterInt(0); // unknown 2
            PushScaleformMovieMethodParameterInt(0); // style/icon
            PushScaleformMovieMethodParameterInt(0); // unknown 3
            PushScaleformMovieMethodParameterInt(0); // unknown 4

            BeginTextCommandScaleformString("FM_NXT_PLY");  // left label
            EndTextCommandScaleformString();

            BeginTextCommandScaleformString("LBD_NUM");     // players label
            AddTextComponentInteger(minPlayers);            // min players
            AddTextComponentInteger(maxPlayers);            // max players
            EndTextCommandScaleformString();

            EndScaleformMovieMethod();
            #endregion


            #region Type row
            int type = jobType;
            int typeAlt = jobType;
            string jobLabel = "FMMC_MPM_TY0"; // mission
            switch (jobType)
            {
                case 0: // Mission
                    type = 2;
                    typeAlt = 0;
                    jobLabel = "FMMC_MPM_TY0";
                    break;

                case 1: // Team Deathmatch
                    type = 2;
                    typeAlt = 4;
                    jobLabel = "FMMC_RSTAR_TDM";
                    break;

                case 2: // Deathmatch
                    type = 2;
                    typeAlt = 1;
                    jobLabel = "FMMC_MPM_TY1";
                    break;

                case 3: // Race
                    type = 2;
                    typeAlt = 2;
                    jobLabel = "FMMC_MPM_TY2";
                    break;

                case 4: // Survival
                    type = 2;
                    typeAlt = 3;
                    jobLabel = "FMMC_MPM_TY4";
                    break;

                default: // None
                    type = 2;
                    typeAlt = 0;
                    break;
            }
            PushScaleformMovieFunction(scaleformHandle, "SET_DETAILS_ITEM");
            PushScaleformMovieMethodParameterInt(3); // row
            PushScaleformMovieMethodParameterInt(0); // unknown 1
            PushScaleformMovieMethodParameterInt(0); // unknown 2
            PushScaleformMovieMethodParameterInt(type); // style/icon
            PushScaleformMovieMethodParameterInt(0); // unknown 3
            PushScaleformMovieMethodParameterInt(0); // unknown 4

            BeginTextCommandScaleformString("FM_NXT_TYP");  // left label
            EndTextCommandScaleformString();

            BeginTextCommandScaleformString(jobLabel);     // Mission type setup
            EndTextCommandScaleformString();
            PushScaleformMovieMethodParameterInt(typeAlt);        // alternate icon type
            PushScaleformMovieMethodParameterInt(9);        // color
            PushScaleformMovieMethodParameterBool(false);    // checkmark
            EndScaleformMovieMethod();
            #endregion


            #region Area row
            PushScaleformMovieFunction(scaleformHandle, "SET_DETAILS_ITEM");
            PushScaleformMovieMethodParameterInt(4); // row
            PushScaleformMovieMethodParameterInt(0); // unknown 1
            PushScaleformMovieMethodParameterInt(0); // unknown 2
            PushScaleformMovieMethodParameterInt(0); // style/icon
            PushScaleformMovieMethodParameterInt(0); // unknown 3
            PushScaleformMovieMethodParameterInt(0); // unknown 4

            BeginTextCommandScaleformString("FM_NXT_ARA");  // left label setup
            EndTextCommandScaleformString();

            BeginTextCommandScaleformString(areaLabel);      // area label setup
            EndTextCommandScaleformString();

            EndScaleformMovieMethod();
            #endregion

        }
        #endregion
        //private void SetDetailsItem(int scaleformHandle, int rowIndex, int unk1, int unk2, int icon, int unk3, int unk4, string leftLabelText, string rightText, bool unk5)
        //{
        //    PushScaleformMovieFunction(scaleformHandle, "SET_DETAILS_ITEM");
        //    PushScaleformMovieMethodParameterInt(rowIndex);   // row index
        //    PushScaleformMovieMethodParameterInt(unk1);       // ?
        //    PushScaleformMovieMethodParameterInt(unk2);       // ?
        //    PushScaleformMovieMethodParameterInt(icon);       // icon/style: 2 = star, 3 = crew tag, 4 = top white line.
        //    PushScaleformMovieMethodParameterInt(unk3);       // ?
        //    PushScaleformMovieMethodParameterInt(unk4);       // ?

        //    BeginTextCommandScaleformString(leftLabelText);     // left text
        //    EndTextCommandScaleformString();

        //    if (leftLabelText == "FM_NXT_RAT")      // rating
        //    {
        //        BeginTextCommandScaleformString("FM_NXT_RAT1"); // rating label
        //        AddTextComponentInteger(int.Parse(rightText));  // rating %
        //        EndTextCommandScaleformString();
        //    }
        //    else if (leftLabelText == "FM_NXT_CRE") // created by
        //    {
        //        N_0xe83a3e3557a56640(rightText); // playername        
        //        BeginTextCommandScaleformString(""); // unused
        //        EndTextCommandScaleformString();
        //        PushScaleformMovieMethodParameterBool(unk5);  // unknown
        //    }
        //    else if (leftLabelText == "FM_NXT_PLY") // players
        //    {
        //        BeginTextCommandScaleformString("LBD_NUM");
        //        AddTextComponentInteger(unk1);
        //        AddTextComponentInteger(unk2);
        //        EndTextCommandScaleformString();
        //    }
        //    else if (leftLabelText == "FM_NXT_TYP") // type
        //    {
        //        BeginTextCommandScaleformString(rightText);
        //        EndTextCommandScaleformString();
        //        PushScaleformMovieMethodParameterInt(0);
        //        PushScaleformMovieMethodParameterInt(9);
        //        PushScaleformMovieMethodParameterBool(unk5);
        //    }
        //    else if (leftLabelText == "FM_NXT_ARA") // area
        //    {
        //        BeginTextCommandScaleformString(rightText);
        //        EndTextCommandScaleformString();
        //    }

        //    EndScaleformMovieMethod();
        //}

        #endregion

    }
}
