using Jypeli;


namespace PuzzleGame;

/// @author turpe
/// @version 04.10.2024
/// <summary>
/// PuzzleGame with item collection system and inventory.
/// </summary>
public class PuzzleGame : PhysicsGame
{
    public PhysicsObject Pelaaja;
    

    public override void Begin()
    {
        Pelaaja = LuoPelaaja(this, Level.Left + 20.0, 0.0);
        AsetaOhjaimet(Pelaaja);
        
        LuoKentta();
        Camera.ZoomToLevel();

        AsetaOhjaimet(Pelaaja);
        
        // Törmäyskäsittelijä pelaajan ja kerättävien esineiden välillä
        //AddCollisionHandler(Pelaaja, "Kerättävä", LisaaEsine);
    }

    protected override void Update(Time deltaTime)
    {
        base.Update(deltaTime); // Kutsu perintäluokan Update-metodia
        Camera.Position = Pelaaja.Position; // Aseta kamera pelaajan sijaintiin
    }
    
    private void LuoKentta()
    {
        double leveys = Screen.Width; // Kentän leveys näytön leveyden mukaan
        double korkeus = leveys * 9 / 16; // Laske korkeus 16:9 -suhteen mukaan
        
        Level.Background.Color = Color.Black; // Aseta taustaväri
        LuoRajatutSeinat(leveys, korkeus);
        Camera.ZoomToLevel(); // Zoomaa kenttä

        PhysicsObject esine = LuoEsine(20, 20);
        Add(esine);
    }
    
    private void LuoRajatutSeinat(double leveys, double korkeus)
    {
        PhysicsObject vasenSeina = PhysicsObject.CreateStaticObject(10, 125 * 4);
        vasenSeina.X = -200 * 2.5;
        vasenSeina.Color = Color.White;
        Add(vasenSeina);

        PhysicsObject oikeaSeina = PhysicsObject.CreateStaticObject(10, 125 * 4);
        oikeaSeina.X = 200 * 2.5;
        oikeaSeina.Color = Color.White;
        Add(oikeaSeina);

        PhysicsObject ylaSeina = PhysicsObject.CreateStaticObject(100 * 10, 10);
        ylaSeina.Y = 125 * 2;
        ylaSeina.Color = Color.White;
        Add(ylaSeina);

        PhysicsObject alaSeina = PhysicsObject.CreateStaticObject(100 * 10, 10);
        alaSeina.Y = -125 * 2;
        alaSeina.Color = Color.White;
        Add(alaSeina);
    }

    public PhysicsObject LuoEsine(double x, double y)
    {
        PhysicsObject esine = new PhysicsObject(20, 20);
        esine.Position = RandomGen.NextVector(Level.BoundingRect);
        esine.Color = Color.Yellow;
        esine.Tag = "Kerättävä"; // Asetetaan tag esineelle
        return esine;
    }
   
    public static PhysicsObject LuoPelaaja(PhysicsGame peli, double x, double y)
    {
        PhysicsObject pelaaja = new PhysicsObject(40.0, 40.0, Shape.Rectangle);
        pelaaja.X = x;
        pelaaja.Y = y;
        peli.Add(pelaaja);
        return pelaaja;
    }
    
    private void AsetaOhjaimet(PhysicsObject pelaaja)
    {
        Keyboard.Listen(Key.W, ButtonState.Pressed, AsetaNopeus, "Liikuta hahmoa ylös", pelaaja, new Vector(0, 200));
        Keyboard.Listen(Key.S, ButtonState.Pressed, AsetaNopeus, "Liikuta hahmoa alas", pelaaja, new Vector(0, -200));
        Keyboard.Listen(Key.A, ButtonState.Pressed, AsetaNopeus, "Liikuta hahmoa vasempaan", pelaaja, new Vector(-200, 0));
        Keyboard.Listen(Key.D, ButtonState.Pressed, AsetaNopeus, "Liikuta hahmoa oikealle", pelaaja, new Vector(200, 0));

        // Stop movement when the key is released
        Keyboard.Listen(Key.W, ButtonState.Released, AsetaNopeus, "Pysäytä liike ylös", pelaaja, new Vector(0, 0));
        Keyboard.Listen(Key.S, ButtonState.Released, AsetaNopeus, "Pysäytä liike alas", pelaaja, new Vector(0, 0));
        Keyboard.Listen(Key.A, ButtonState.Released, AsetaNopeus, "Pysäytä liike vasempaan", pelaaja, new Vector(0, 0));
        Keyboard.Listen(Key.D, ButtonState.Released, AsetaNopeus, "Pysäytä liike oikealle", pelaaja, new Vector(0, 0));
    }
    
    public static void AsetaNopeus(PhysicsObject pelaaja, Vector nopeus)
    {
        pelaaja.Velocity = nopeus;
    }

    // Törmäyskäsittelijä, joka hoitaa esineiden keräämisen
    /*private void LisaaEsine(PhysicsObject pelaaja, PhysicsObject esine)
    {
        if (_inventory.AddItem(esine.Name ?? "Tuntematon esine"))
        {
            Remove(esine); // Poista esine kentältä, jos lisäys onnistuu
        }
    }*/

   
}
