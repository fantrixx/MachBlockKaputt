using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using AlleywayMonoGame.Entities;
using AlleywayMonoGame.Models;
using AlleywayMonoGame.Managers;
using System.Collections.Generic;

namespace AlleywayMonoGame.Input
{
    /// <summary>
    /// Handles all input processing for gameplay and UI interactions
    /// </summary>
    public class InputHandler
    {
        private KeyboardState _previousKeyState;
        private MouseState _previousMouseState;

        public InputHandler()
        {
            _previousKeyState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();
        }

        public void UpdatePreviousStates()
        {
            _previousKeyState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();
        }

        public KeyboardState GetCurrentKeyboardState() => Keyboard.GetState();
        public KeyboardState GetPreviousKeyboardState() => _previousKeyState;

        /// <summary>
        /// Handles paddle movement input
        /// </summary>
        public void HandlePaddleInput(Paddle paddle, float deltaTime)
        {
            var kb = Keyboard.GetState();
            
            if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A))
            {
                paddle.MoveLeft(deltaTime);
            }
            else if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D))
            {
                paddle.MoveRight(deltaTime);
            }
            else
            {
                paddle.Stop();
            }
        }

        /// <summary>
        /// Handles ball launch input
        /// </summary>
        public bool CheckBallLaunchInput()
        {
            var kb = Keyboard.GetState();
            return kb.IsKeyDown(Keys.Space);
        }

        /// <summary>
        /// Handles projectile shooting input
        /// </summary>
        public bool CheckShootInput()
        {
            var kb = Keyboard.GetState();
            return kb.IsKeyDown(Keys.Space) && _previousKeyState.IsKeyUp(Keys.Space);
        }

        /// <summary>
        /// Checks for cheat code inputs
        /// </summary>
        public (bool winLevel, bool winAll) CheckCheatCodes()
        {
            var kb = Keyboard.GetState();
            bool winLevel = kb.IsKeyDown(Keys.P) && _previousKeyState.IsKeyUp(Keys.P);
            bool winAll = kb.IsKeyDown(Keys.O) && _previousKeyState.IsKeyUp(Keys.O);
            return (winLevel, winAll);
        }

        /// <summary>
        /// Handles shop interaction input
        /// </summary>
        public ShopInputResult HandleShopInput(UIManager uiManager, bool purchaseAnimationActive, bool moneyAnimationDone)
        {
            var result = new ShopInputResult();
            var mouseState = Mouse.GetState();
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            // Update hover states
            for (int i = 0; i < 3; i++)
            {
                uiManager.ShopButtonsHovered[i] = uiManager.ShopButtons[i].Contains(mousePos);
            }
            uiManager.NextLevelButtonHovered = uiManager.NextLevelButton.Contains(mousePos);
            uiManager.RerollButtonHovered = uiManager.RerollButton.Contains(mousePos);

            // Check for clicks (only on button press, not hold)
            if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released && !purchaseAnimationActive && moneyAnimationDone)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (uiManager.ShopButtonsHovered[i])
                    {
                        result.ShopItemClicked = i;
                        result.ShopButtonClicked = true;
                        break;
                    }
                }

                if (uiManager.NextLevelButtonHovered)
                {
                    result.NextLevelClicked = true;
                }

                if (uiManager.RerollButtonHovered)
                {
                    result.RerollClicked = true;
                }
            }

            _previousMouseState = mouseState;
            return result;
        }

        /// <summary>
        /// Handles game over and victory screen input
        /// </summary>
        public DialogInputResult HandleDialogInput(UIManager uiManager)
        {
            var result = new DialogInputResult();
            var mouseState = Mouse.GetState();
            Point mousePos = new Point(mouseState.X, mouseState.Y);

            // Update hover states
            uiManager.RetryButtonHovered = uiManager.RetryButton.Contains(mousePos);
            uiManager.QuitButtonHovered = uiManager.QuitButton.Contains(mousePos);
            uiManager.VictoryRetryButtonHovered = uiManager.VictoryRetryButton.Contains(mousePos);
            uiManager.VictoryQuitButtonHovered = uiManager.VictoryQuitButton.Contains(mousePos);

            // Check for clicks
            if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                if (uiManager.RetryButtonHovered || uiManager.VictoryRetryButtonHovered)
                {
                    result.RetryClicked = true;
                }
                if (uiManager.QuitButtonHovered || uiManager.VictoryQuitButtonHovered)
                {
                    result.QuitClicked = true;
                }
            }

            _previousMouseState = mouseState;
            return result;
        }
    }

    /// <summary>
    /// Result of shop input handling
    /// </summary>
    public struct ShopInputResult
    {
        public bool ShopButtonClicked;
        public int ShopItemClicked;
        public bool NextLevelClicked;
        public bool RerollClicked;
    }

    /// <summary>
    /// Result of dialog input handling
    /// </summary>
    public struct DialogInputResult
    {
        public bool RetryClicked;
        public bool QuitClicked;
    }
}
