public static class UIDialogueStorage
{
    public static string FoundLoot = "There is a treasure chest!";
    public static string InventoryFull = "Your inventory is already full.";
    public static string PassingTheGate = "As you pass the gate, the watchful eyes of a guard set upon you.";
    public static string LetPassed = "He lets you enter.";
    public static string InfrontOfTavern = "The tavern seems quite inviting.";
    public static string[] SleepDeprivedLines =
        {"You feel weakened by your lack of sleep."};
    public static string[] HealedSleepDeprivedLines =
        {"Your body has regained its agility."};
    public static string[] VampireLines =
        {"You feel... a hunger.",
        "A hunger for BLOOD!"};
    public static string[] HealedVampireLines =
        {"Your skin regains its color.",
        "This hunger that has been driving you mad lately seems to have vanished entirely."};
    public static string[] VampireSunDamageLines =
        {"The sun burns into your skin.",
        $"You take {GameConfig.VampireSunDamage} damage."};
    public static string[] WerewolfLines =
        {"Was your arm always this hairy?",
        "Your teeth feel like they are growing.",
        "This is not good!",
        "But you feel strong. So incredibly strong!"};
    public static string[] HealedWerewolfLines =
        {"The fur seems to be receding.",
        "Your aggression gets replaced with a distinct dizziness.",
        "You need to sleep and not dream about the moon."};
    public static string[] ZombieLines =
        {"The first thing you notice is the strange smell of decaying flesh.",
        "And then for some reason you think about brains. Tasty brains.",
        "And that's the last clear thought you've ever had."};
    public static string[] HealedZombieLines =
        {"You truly don't remember anything.",
        "You have a weird taste in your mouth for some reason.",
        "Welcome back from the dead."};
    public static string[] WerewolfNightLines =
        {"As the sun disappears, you feel the fur growing out of your back again.",
        "You gaze at the full moon and you let out a haunting howl.",
        "They fear you - and you love it."};
    public static string[] WerewolfDayLines =
        {"As the sun rises, you slowly calm down.",
        "All the anger vanishes, just like the fur on your body.",
        "You seem to be your normal non-werewolf self again."};
    public static string[] ZombieInsultAttemptLines =
        {"You: 'Grrrrr!!!!!'",
        "This seems to be all your vocal chords can produce.",
        "A rotting brain doesn't think very well.",
        "You shouldn't try this again."};
    public static string[] ZombieInsultResistLines =
        {"You don't understand a word.",
        "Ignorance truly is bliss."};
    public static string[] StayingOutsideOfTownLines =
        {"You make camp just outside of town.",
        "The sky is clear and a million stars twinkle in the sky.",
        "Somewhere a dog barks behind the town's walls.",
        "It's cold, but you light a fire to keep warm."};
    public static string[] ReachedTavernLines =
        {"You enter and sit down at a table."};
    public static string[] AfterTavernDialogue =
        {"You are alone again."};
    public static string[] GettingCaughtAtTheGateLines =
        {"He looks at you for a long while, studying you.",
        "For a moment he seems unsure, but then he shouts at the top of his lungs:"};
    public static string[] VampireInTheCityLines =
        {"Being in town with so many warm bodies around gives you chills.",
        "You can almost taste the blood pulsing through their veins.",
        "Drinking some of that tasty scarlet nectar would surely boost your vitality for some time.",
        "Should you do it?",
        "Bite someone?"};
    public static string[] VampireLookingForVictimLines =
        {"In this room you can hear the loud snoring of someone in deep slumber.",
        "The room at the end of the hall on the first floor is quite secluded.",
        "The smell of the person in the room near the stairs is just too good. You decide to enter." };
    public static string[] VampireCaughtLines =
        {"Just as you hunch down over your sleeping victim, their eyes open wide and they scream loudly.",
        "You take too long trying to pick the lock. A passerby spots you and calls for the guards.",
        "You trip over a cat before reaching the bed. The cat's cry gives you away.",
        "You failed to notice the maid in the corner of the lady's room. Her screams alarm the guards." };
    public static string[] VampireBiteLines =
        {"The taste of blood overwhelms you.",
        "You enjoy every drop while you feel your body grow stronger.",
        "With the back of your hand you wipe the blood off your lips",
        "You head back to the main room of the tavern with a big, satisfied grin on your face."};
    public static string[] VampireBoostLossLines =
        {"The effects of your last blood feast seem to vanish.",
        "Your strength appears to have returned to a normal level."};
    public static string[] GuardAppearsLines =
        {"Guard: 'I will banish you from this town, you monster!'",
        "Guard: 'My blade will end your life, you pitiful creature!'",
        "Guard: 'Die and burn in the pits of hell for all eternity!'",
        "Guard: 'I shall defend the citizens of this town with my life!'"};
    public static string[] SlayedGuardLines =
        {"Somehow you managed to slay the guard.",
        "But now the whole town is alerted!",
        "You have to flee and camp outside of town."};
    public static string[] EscapedGuardLines =
        {"Somehow you managed to escape the guard.",
        "But now the whole town is alerted!",
        "You have to flee and camp outside of town."};
    public static string[] EnemyFleeLines =
        {"I don't want to be a zombie!",
        "Screw this! This is not worth it!",
        "Good luck to you, I'm out of here!"};
    public static string[] PondReachedLines =
        {"You reach a small pond.",
        "The world around you is silent."};
    public static string[] PondEntryLines =
        {"You take off your armor.",
        "Carefully, you enter the pond, step by step, until the cool water covers most of your body.",
        "You feel your ailments wash away."};
    public static string[] ReadyToAttackBossLines =
        {"You have rid yourself of all your ego.",
        "The voice seems confused.",
        "Something is different about it."};
    public static string[] BossDefeatedLines =
        {"You have won the fight!",
        "You have finished your quest!"};
    public static string[] MeetingDogLines =
        {"You see a dog coming towards you.",
        "He stops infront of you, eyeing you curiously.",
        "What will you do?"};
    public static string[] MeetingDogAgainLines =
        {"You see the dog again.",
        "What will you do?"};
    public static string[] PetDogLines =
        {"You pet the dog slowly.",
        "His fur is rough and bit dirty.",
        "His expression is hard to read, so you can't really tell if he's actually enjoying it."};
    public static string[] PetDogAgainLines =
        {"You should know by now that I'm not that kind of dog."};
    public static string[] ZombieCantSpeakLines =
        {"You growl a bit in an embarassing attempt to make conversation."};
    public static string[] DogSaveLines =
        {"You hear barking just as the ominous shadow is about to attack you.",
        "The dog you met earlier takes a brave stand infront of you, barking ferociously.",
        "The shadow vanishes and the dog chases it into the bushes.",
        "It seems you've made a friend."};
    public static string[] EndLines =
        {"Created by Zara Cakir and Lukas Bohl.",
        "Thank you for playing.",
        "Until next time, dear knight."};
}
