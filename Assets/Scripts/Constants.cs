using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Constants
{
    public static readonly int ROWS = 12;
    public static readonly int COLUMNS = 8;
    public static readonly float ANIMATION_DURATION =  0.2f;

    public static readonly float MOVE_ANIMATION_MIN_DURATION = 0.05f;

    public static readonly float EXPLOSION_DURATION = 0.3f;

    public static readonly float WAIT_BEFORE_POTENTIAL_MATCHES_CHECK = 2f;
    public static readonly float OPACITY_ANIMATION_FRAME_DELAY = 0.05f;

    public static readonly int MINIMUM_MATCHES = 3;
    public static readonly int MINIMUM_MATCHES_FOR_BONUS = 4;

    public static readonly int MATCH_THREE_SCORE = 60;
    public static readonly int SUBSEQUENT_MATCH_SCORE = 1000;
}