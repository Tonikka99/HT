using System;
using System.Diagnostics.CodeAnalysis;
using Jypeli;


namespace RiddleGame;

/// @author Antturpe
/// @version 21.11.2024
/// <summary>
/// 
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")] // vammainen "_" ehdotus poistetu
public class RiddleGame : PhysicsGame
{
    private PhysicsObject pelaaja;
    private Timer peliAjastin;
    private IntMeter aika;
    private IntMeter keratty;
    
    
    /// <summary>
    /// 
    /// </summary>
    public override void Begin()
    {
        LuoIntroKuva();
        
        pelaaja = LuoPelaaja(this, Level.Left + 20, 0);
        AsetaOhjaimet(pelaaja);
        LuoKello();
        
        LuoRajatutSeinat();
        
        //Luodaan ajastin ja laitetaan se näkyväksi 
        aika = new IntMeter(40);
        Label aikalabel = new Label();
        aikalabel.BindTo(aika);
        aikalabel.TextColor = Color.Red;
        aikalabel.Position = new Vector(0, Screen.Top - 40);
        Add(aikalabel);
        
        
        // Luodaan tarkistin joka tarkastaa aikaa ja lopettaa pelin jos aika loppuu
        peliAjastin = new Timer();
        peliAjastin.Interval = 1.0; // Laskee sekunteja
        peliAjastin.Timeout += VahennaAikaa;
        peliAjastin.Start();
        
        LuoAjastin(5.0, LuoKello);
        keratty = KeratytKellot();
        AddCollisionHandler(pelaaja, "kello", TormaysKelloon);
        
        puzzle1(pelaaja);
    }
    

    /// <summary>
    /// 
    /// </summary>
    private void LuoIntroKuva()
    {
        Image tarinaKuva = LoadImage("tarinaKuva");
    }
    
    private void LuoAjastin(double intervalli, Action toiminto)
    {
        Timer ajastin = new Timer();
        ajastin.Interval = intervalli;
        ajastin.Timeout += toiminto;
        ajastin.Start();
    }

    
    
    /// <summary>
    /// Luo uuden kellon ajastimen avulla.
    /// </summary>
    private void LuoKello()
    {
        // Rajojen sisäpuolelle jäävä alue
        double minX = Level.Left + 40;  // Esineen leveys
        double maxX = Level.Right - 40;
        double minY = Level.Bottom + 40;  // Esineen korkeus
        double maxY = Level.Top - 40;

        // Arvotaan satunnainen sijainti rajojen sisällä
        double randomX = RandomGen.NextDouble(minX, maxX);
        double randomY = RandomGen.NextDouble(minY, maxY);

        // Luo esine
        int elinika = 10000;
        PhysicsObject kello = new PhysicsObject(50, 50);  // Esineen koko (leveys, korkeus)
        kello.Position = new Vector(randomX, randomY);  // Sijoita satunnaisesti
        kello.Color = Color.Yellow;  // Esineen väri
        kello.Tag = "kello";  // Asetetaan tag esineelle, joka voidaan tunnistaa
        kello.CanRotate = false;  // Estetään esineen pyöriminen
        kello.LifetimeLeft = TimeSpan.FromMilliseconds(elinika);
        
        kello.Image = LoadImage("clock");  // Ladataan kuvan esineelle
        kello.Height = 100;
        kello.Width = 100; // Kuvan koko
        kello.IgnoresCollisionResponse = true;
        
        Add(kello);  // Lisätään esine peliin
    }

    
    /// <summary>
    /// Luodaan pelaaja hahmo
    /// </summary>
    /// <param name="peli">Pelattava peli</param>
    /// <param name="x">Pelaajan koko</param>
    /// <param name="y">Pelaajan koko</param>
    /// <returns>Palauttaa hahmon peliin</returns>
    private static PhysicsObject LuoPelaaja(PhysicsGame peli, double x, double y)
    {
        PhysicsObject pelaaja = new PhysicsObject(40.0, 40.0, Shape.Rectangle);
        pelaaja.X = x;
        pelaaja.Y = y;
        pelaaja.CanRotate = false;
        pelaaja.IgnoresCollisionResponse = true;
        peli.Add(pelaaja);
        return pelaaja;
    }


    /// <summary>
    /// Lisätään aikaa pelaajalle ja poistetaan kello pelistä
    /// </summary>
    /// <param name="hahmo">Pelattava hahmo</param>
    /// <param name="kello">Aikaa lisäävä esine</param>
    private void TormaysKelloon(PhysicsObject hahmo, PhysicsObject kello)
    {
        aika.Value += 15;
        keratty.Value ++; 
        Remove(kello);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IntMeter KeratytKellot()
    {
        keratty = new IntMeter(0); // Luodaan laskuri
        Label keratytlabel = new Label();
        keratytlabel.BindTo(keratty); // Sidotaan laskuri labeliin
        keratytlabel.TextColor = Color.Red; // Määritetään väri
        keratytlabel.Position = new Vector(Screen.Right - 100, Screen.Top - 40); // Sijoitetaan label
        Add(keratytlabel); // Lisätään label peliin
        return keratty; // Palautetaan laskuri
    }

    
    
    /// <summary>
    /// Ohjelma tarkastelee aikaa ja ajaa lopetus mikäli aika loppuu
    /// </summary>
    private void VahennaAikaa()
    {
        aika.Value--;
        if (aika.Value <= 0)
        {
            peliAjastin.Stop();
            LuoAjastin(5, LuoKello);
            Havisit();
        }
    }
    
    
    /// <summary>
    /// Luodaan pelaajalle kyky liikkua
    /// </summary>
    /// <param name="pelaaja">pelattava hahmo</param>
    private void AsetaOhjaimet(PhysicsObject pelaaja)
    {
        Vector[] suunnat = {
            new (0, 200),  // Ylös (W)
            new (0, -200), // Alas (S)
            new (-200, 0), // Vasemmalle (A)
            new (200, 0)   // Oikealle (D)
        };

        Key[] nappaimet = { Key.W, Key.S, Key.A, Key.D };

        for (int i = 0; i < nappaimet.Length; i++)
        {
            Keyboard.Listen(nappaimet[i], ButtonState.Pressed, AsetaNopeus, null, pelaaja, suunnat[i]);
            Keyboard.Listen(nappaimet[i], ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        }

        Keyboard.Listen(Key.H, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        
        //TODO: InspectItem ohjelma jolla voi katsella kerättyä esinettä.
        //Keyboard.Listen(Key.F, ButtonState.Pressed, InspectItem, "Tarkastele esinettä", pelaaja)
    }

    
    
    /// <summary>
    /// Luodaan pelaajalle nopeus
    /// </summary>
    /// <param name="pelaaja">Pelattava hahmo</param>
    /// <param name="nopeus">Annettaan pelaajalle nopeus</param>
    private static void AsetaNopeus(PhysicsObject pelaaja, Vector nopeus)
    {
        pelaaja.Velocity = nopeus;
    }
    
    
    /// <summary>
    /// Luodaan rajatut seinät 
    /// </summary>
    private void LuoRajatutSeinat()
    {
        Level.CreateLeftBorder(0, true);
        Level.CreateBottomBorder(0, true);
        Level.CreateRightBorder(0, true);
        Level.CreateTopBorder(0, true);
        
        //var a = Level.CreateTopBorder(0, true);
        //a.Height = 100;
        // Säädetään korkeus ja leveys
    }
    

    /// <summary>
    /// Häviämiseen liittyvä "fade to black" efekti
    /// </summary>
    private void Havisit()
    {
        // Asetetaan kuva koko näytölle
        PhysicsObject loppu = new PhysicsObject(Level.Width, Level.Height);
        loppu.X = Level.Center.X;
        loppu.Y = Level.Center.Y;
        loppu.Image = LoadImage("the_end");
        Add(loppu);
    
        // Voit lisätä myös muita toimintoja, kuten peliin äänen toiston
        // Game.PlaySound("KoiranAani1");
        
    }


    /// <summary>
    /// Lasketaan onko kelloja kerätty oikea määrä ensimmäisen tehtävän avaamiseksi 
    /// </summary>
    /// <param name="keratty">kerättävä objekti</param>
    /// <returns>palauttaa true tai false</returns>
    /// <example>
    /// <pre name="test">
    /// Keraily(3) === false;
    /// Keraily(5) === true;
    /// Keraily(10) === true;
    /// </pre>
    /// </example>
    private static bool Keraily(int keratty)
    {
        return keratty >= 5;
    }
    
    
    protected override void Update(Time time)
    {
        base.Update(time);
        puzzle1(pelaaja); // Tarkistetaan ehdot jokaisella framella
    }
    
    

    public void puzzle1(PhysicsObject pelaaja)
    {
        if (aika.Value <= 30)
        {
            Console.WriteLine("Se toimii yippeee");
        }
    }
    
    
}

