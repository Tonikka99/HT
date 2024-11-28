using System;
using System.Collections.Generic;
using Jypeli;
using Vector = Jypeli.Vector;

// ReSharper disable All


namespace RiddleGame;

/// @author Antturpe
/// @version 27.11.2024
/// <summary>
/// Hassun hauska ja haastava riddleGame
/// </summary>

public class RiddleGame : PhysicsGame
{
    private PhysicsObject _pelaaja;
    private Timer peliAjastin;
    private Timer kelloAjastin;
    private IntMeter aika;
    private IntMeter keratty;
    private bool task1;
    private bool task2;
    private bool nayttoLuotu = false;
    private string pelaajanVastaus = ""; //Vastaus 1815.


    public override void Begin()
    {
        AsetaTausta();
        LuoIntroKuva();

        _pelaaja = LuoPelaaja(Level.Left + 20, 0);
        LuoKello();

        LuoRajatutSeinat();

        PeliAika();
        LuoAjastin(3.0, LuoKello);
        keratty = KeratytKellot(keratty,this);

        Puzzle1(_pelaaja);
        Puzzle2(task1);
        LuoLukkoJaTormays();
        
        AddCollisionHandler(_pelaaja, "kello", TormaysKelloon);
        AddCollisionHandler(_pelaaja, "lukko", AloitaNumeronArvaus);
    }

    /// <summary>
    ///  Luodaan ajastin ja laitetaan se näkyväksi
    /// </summary>
   private void PeliAika()
   {
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

   }

    /// <summary>
    /// Luodaan pelille background kuva
    /// </summary>
    private void AsetaTausta()
    {
        PhysicsObject tausta = new PhysicsObject(Level.Width, Level.Height);
        tausta.Y = Level.Center.Y;
        tausta.X = Level.Center.X;
        tausta.Image = LoadImage("karttaLayout");
        tausta.IgnoresCollisionResponse = true;
        Add(tausta, -1);

    }
    
    
    /// <summary>
    /// Luo keskelle ruutua yläosaan tekstin, jota voi käyttää pelin eri vaiheissa.
    /// </summary>
    /// <param name="teksti">Näytettävä teksti</param>
    /// <param name="koko">Tekstin koko</param>
    /// <returns>Palauttaa luodun Label-olion</returns>
    public Label Teksti(string teksti, double koko = 40)
    {
        Label label = new Label(teksti)
        {
            Position = new Vector(0, Screen.Top - 70), // Keskellä ruudun yläosaa

        };
        label.Color = Color.Red;
        Timer.SingleShot(2.0, () => label.Destroy());
        
        Add(label);
        return label; // Palautetaan label, jotta sen ominaisuuksia voi muokata myöhemmin
    }


    /// <summary>
    /// Luodaan aluksi kuva joka avaa pelin "tarinaa" ja ehkä antaa vinkin
    /// </summary>
    private void LuoIntroKuva()
    {
        // Luodaan kuva-objekti, joka kattaa koko tason
        PhysicsObject tarinaKuva = new PhysicsObject(Level.Width, Level.Height);
        tarinaKuva.Position = Level.Center;
        tarinaKuva.Image = LoadImage("tarinaKuva");
        tarinaKuva.Tag = "IntroKuva";
        tarinaKuva.CanRotate = false;
        Add(tarinaKuva);

        // Lisätään kuuntelijat WASD-näppäimille, jotka poistavat kuvan
        Keyboard.Listen(Key.W, ButtonState.Pressed, () => PoistaKuva(tarinaKuva), "up");
        Keyboard.Listen(Key.A, ButtonState.Pressed, () => PoistaKuva(tarinaKuva), "Left");
        Keyboard.Listen(Key.S, ButtonState.Pressed, () => PoistaKuva(tarinaKuva), "down");
        Keyboard.Listen(Key.D, ButtonState.Pressed, () => PoistaKuva(tarinaKuva), "right");
    }


    /// <summary>
    /// Poistaa kuvan ja aloittaa ajastimen
    /// </summary>
    /// <param name="kuva">Alkuteksti kuva "intro"</param>
    private void PoistaKuva(GameObject kuva)
    {
        peliAjastin.Start();
        kelloAjastin.Start();
        kuva.Destroy();
        Keyboard.Clear();
        AsetaOhjaimet(_pelaaja);
    }


    /// <summary>
    /// Luodaan peliin ajastin
    /// </summary>
    /// <param name="intervalli">Haluttu toiminto 5sec välein</param>
    /// <param name="toiminto">kello</param>
     void LuoAjastin(double intervalli, Action toiminto)
    {
        if (task1 == false) // Tarkistaa, onko ajastin jo olemassa
        {
            kelloAjastin = new Timer(); // Luo uusi ajastin
            kelloAjastin.Interval = intervalli;
            kelloAjastin.Timeout += toiminto;
        }
    }


    /// <summary>
    /// Luo uuden kellon ajastimen avulla.
    /// </summary>
    private void LuoKello()
    {
        // Rajojen sisäpuolelle jäävä alue
        double minX = Level.Left + 40; // Esineen leveys
        double maxX = Level.Right - 80;
        double minY = Level.Bottom + 40; // Esineen korkeus
        double maxY = Level.Top - 40;

        // Arvotaan satunnainen sijainti rajojen sisällä
        double randomX = RandomGen.NextDouble(minX, maxX);
        double randomY = RandomGen.NextDouble(minY, maxY);

        // Luo esine
        int elinika = 10000;
        PhysicsObject kello = new PhysicsObject(30, 30); // Esineen koko (leveys, korkeus)
        kello.Position = new Vector(randomX, randomY); // Sijoita satunnaisesti
        kello.Color = Color.Yellow; // Esineen väri
        kello.Tag = "kello"; // Asetetaan tag esineelle, joka voidaan tunnistaa
        kello.CanRotate = false; // Estetään esineen pyöriminen
        kello.LifetimeLeft = TimeSpan.FromMilliseconds(elinika);

        kello.Image = LoadImage("clock");
        kello.IgnoresCollisionResponse = true;

        Add(kello);
    }

    
    /// <summary>
    /// Luodaan näyttö ja laitetaan se esille mikäli ensimmäinen tehtävä on suoritettu 
    /// </summary>
    private void LuoNaytto()
    {
        if (nayttoLuotu == false)
        {
            
            PhysicsObject naytto = new PhysicsObject(60, 60);
            naytto.Position = new Vector(Level.Right - 100, Level.Top - 270);
            naytto.Image = LoadImage("KoodiNaytto");
            naytto.CanRotate = false;
            naytto.IgnoresCollisionResponse = true;
            
            nayttoLuotu = true;
            Add(naytto);
        }
            
        
    }


    /// <summary>
    /// Luodaan pelaaja hahmo
    /// </summary>
    /// <param name="x">Pelaajan koko</param>
    /// <param name="y">Pelaajan koko</param>
    /// <returns>Palauttaa hahmon peliin</returns>
    private PhysicsObject LuoPelaaja(double x, double y)
    {
        // Luo pelaaja-objekti
        PhysicsObject pelaaja = new PhysicsObject(100.0, 100.0, Shape.Rectangle)
        {
            X = x,
            Y = y,
            CanRotate = false,
            IgnoresCollisionResponse = true
        };

        // Lataa animaation framet
        Image[] animaatioKuvat =
        {
            LoadImage("hahmoLiike1"),
            LoadImage("hahmoLiike2"),
        };

        // Luo animaatio
        Animation animaatio = new Animation(animaatioKuvat)
        {
            FPS = 10 // Aseta animaation nopeus (framea sekunnissa)
        };

        // Lisätään animaatio pelaajalle
        pelaaja.Animation = animaatio;

        // Pelaaja ei ole vielä liikkumassa, animaatio on pysäytetty
        pelaaja.Animation.Stop();

        // Lisätään pelaaja peliin
        Add(pelaaja);

        // Seurataan pelaajan liikettä
        Timer liikuntaTarkistus = new Timer();
        liikuntaTarkistus.Interval = 0.1; // Tarkistetaan liikkumista useammin, esim. 0.1 sekunnin välein
        liikuntaTarkistus.Timeout += () =>
        {
            if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.W) ||
                Keyboard.IsKeyDown(Key.S))
            {
                if (!pelaaja.Animation.IsPlaying) // Jos animaatio ei ole jo käynnissä
                {
                    pelaaja.Animation.Start(); // Käynnistä animaatio
                }
            }
            else
            {
                if (pelaaja.Animation.IsPlaying) // Jos animaatio on käynnissä ja pelaaja ei liiku
                {
                    pelaaja.Animation.Stop(); // Pysäytä animaatio
                }
            }
        };
        liikuntaTarkistus.Start();

        return pelaaja;
    }


    /// <summary>
    /// Lisätään aikaa pelaajalle ja poistetaan kello pelistä
    /// </summary>
    /// <param name="hahmo">Pelattava hahmo</param>
    /// <param name="kello">Aikaa lisäävä esine</param>
    private void TormaysKelloon(PhysicsObject hahmo, PhysicsObject kello)
    {
        aika.Value += 10;
        keratty.Value++;
        Remove(kello);
    }


    /// <summary>
    /// Laskuri joka kertoo montako kelloa olet kerännyt TODO: Myöhemmin myös scoreboard
    /// </summary>
    /// <returns>palauttaa laskurin</returns>
    private static IntMeter KeratytKellot(IntMeter keratty, Game peli)
    {
        keratty = new IntMeter(0); // Luodaan laskuri
        Label keratytlabel = new Label();
        keratytlabel.BindTo(keratty); // Sidotaan laskuri labeliin
        keratytlabel.TextColor = Color.Red; // Määritetään väri
        keratytlabel.Position = new Vector(Screen.Right - 100, Screen.Top - 40); // Sijoitetaan label
        peli.Add(keratytlabel); // Lisätään label peliin
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
            kelloAjastin.Stop();
            LuoAjastin(50000000, LuoKello);
            Havisit();
        }
    }


    /// <summary>
    /// Luodaan pelaajalle kyky liikkua
    /// </summary>
    /// <param name="pelaaja">pelattava hahmo</param>
    private void AsetaOhjaimet(PhysicsObject pelaaja)
    {
        List<Vector> suunnat = new List<Vector>
        {
            new Vector(0, 200),   // Ylös (W)
            new Vector(0, -200),  // Alas (S)
            new Vector(-200, 0),  // Vasemmalle (A)
            new Vector(200, 0)    // Oikealle (D)
        };

        Key[] nappaimet = { Key.W, Key.S, Key.A, Key.D };

        for (int i = 0; i < nappaimet.Length; i++)
        {
            Keyboard.Listen(nappaimet[i], ButtonState.Pressed, AsetaNopeus, null, pelaaja, suunnat[i]);
            Keyboard.Listen(nappaimet[i], ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        }

        Keyboard.Listen(Key.H, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Enter, ButtonState.Pressed, TarkistaVastaus, "Tarkista pelaajan vastaus");
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
        Level.CreateLeftBorder(0, false);
        Level.CreateBottomBorder(0, false);
        Level.CreateRightBorder(0, false);
        Level.CreateTopBorder(0, false);

        _pelaaja.IgnoresCollisionResponse = false;

        //var a = Level.CreateTopBorder(0, true);
        //a.Height = 100;
        // Säädetään korkeus ja leveys
    }


    /// <summary>
    /// Hävisit pelin ja the end screen
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
    /// Tarkastellaan aikaa jokaisella framella
    /// </summary>
    /// <param name="time">aika</param>
    protected override void Update(Time time)
    {
        base.Update(time);
        Puzzle1(_pelaaja);
    }


    /// <summary>
    /// Ensimmäinen puzzle1. Pelaajan pitää kerätä 5 kelloa ja odottaa että aika laskee alle 30
    /// </summary>
    /// <param name="hahmo">pelattava hahmo</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Puzzle1(PhysicsObject hahmo)
    {
        
        if (keratty.Value >= 5)
        {
            task1 = true;
            LuoNaytto();
        }
        else task1 = false;
    }
    
    
    /// <summary>
    /// Tehtävä 2 jossa pitää laittaa oikeat luvut jotta peli etenee
    /// </summary>
    /// <param name="t1">tarkastellaan aiempaa tehtävää</param>
    private void Puzzle2(bool t1)
    {
        if (t1)
        {
            LuoLukkoJaTormays();

            // Kuuntele numeron lisäystä ja poistamista
            Keyboard.Listen(Key.Enter, ButtonState.Pressed, TarkistaVastaus, "Tarkista pelaajan vastaus");
            Keyboard.Listen(Key.Back, ButtonState.Pressed, PoistaNumero, "Poista viimeinen numero");
            for (int i = 0; i <= 9; i++)
            {
                int numero = i; // Lukitse paikallinen muuttuja delegaatille
                Keyboard.Listen((Key)Enum.Parse(typeof(Key), "D" + numero), ButtonState.Pressed, LisaaNumero,
                    "Lisää numero " + numero, numero.ToString());
            }
        }
    }
    
    
    /// <summary>
    /// Luo lukko-objektin peliin ja lisää törmäyksen käsittelijän.
    /// </summary>
    private void LuoLukkoJaTormays()
    {
        // Luo lukko-objekti
        PhysicsObject lukko = new PhysicsObject(150, 250, Shape.Rectangle);
        lukko.Position = new Vector(Level.Right -75, Level.Top - 310);
        lukko.Tag = "lukko";
        lukko.IsVisible = false;
        lukko.CanRotate = false;
        lukko.IgnoresCollisionResponse = true;
        Add(lukko);

        // Lisää törmäyksen käsittelijä pelaajan ja lukon välille
        AddCollisionHandler(_pelaaja, lukko, AloitaNumeronArvaus);
    }
    

    /// <summary>
    /// Käynnistää numeron arvaustehtävän, kun pelaaja osuu lukkoon.
    /// </summary>
    /// <param name="pelaaja">Pelaajan hahmo</param>
    /// <param name="lukko">Lukko-objekti</param>
    private void AloitaNumeronArvaus(PhysicsObject pelaaja, PhysicsObject lukko)
    {
        if (task2 == false && task1 == true)
        {
            Teksti("You found a lock! Guess the code to open it.");

            // Kuuntele pelaajan syöttämät numerot ja vastaukset
            Keyboard.Listen(Key.E, ButtonState.Pressed, AloitaInput, "Aloita pelaajan vastaus");
        }
    }


    /// <summary>
    /// Poistetaan tilapäisesti ohjaimet ja luodaan uudet jotta voidaan antaa vastaus
    /// </summary>
    private void AloitaInput()
    {
        MessageDisplay.Add("numpad not available.");
        
        Keyboard.Clear();
        
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, TarkistaVastaus, "Tarkista pelaajan vastaus");
        Keyboard.Listen(Key.Back, ButtonState.Pressed, PoistaNumero, "Poista viimeinen numero");
        for (int i = 0; i <= 9; i++)
        {
            int numero = i; // Lukitse paikallinen muuttuja delegaatille
            Keyboard.Listen((Key)Enum.Parse(typeof(Key), "D" + numero), ButtonState.Pressed, LisaaNumero,
                "Lisää numero " + numero, numero.ToString());
        }
    }
    
    
    /// <summary>
    /// Lisätään numero sarjaan jota käytetään koodilukossa
    /// </summary>
    /// <param name="numero">lisättävä numero</param>
    void LisaaNumero(string numero)
    {
        pelaajanVastaus += numero; // Lisää syötetty numero vastaukseen
    }
    
    
    /// <summary>
    /// Poistaa numeron merkkijonosta
    /// </summary>
    void PoistaNumero()
    {
        if (pelaajanVastaus.Length > 0)
        {
            pelaajanVastaus = pelaajanVastaus.Substring(0, pelaajanVastaus.Length - 1);
        }
    }


    /// <summary>
    /// Tarkistaa onko vastaus oikein ja käynnistää ohjelmia vastausten mukaan
    /// </summary>
    void TarkistaVastaus()
    {
        if (task1 == true)
        {
            if (pelaajanVastaus == "1815")
            {
                AsetaOhjaimet(_pelaaja);
                Teksti("Door Opened");
                task2 = true;
                Keyboard.Listen(Key.E, ButtonState.Pressed, LoppuTeksti, ":)");
            }
            else
            {
                AsetaOhjaimet(_pelaaja);
                Teksti("*peep sound_effect. Much spoopy*");
            }
        }
    }
    

    /// <summary>
    /// Jekkuna pitää vielä painaa E kerran jotta pääsee ulos
    /// </summary>
    private void LoppuTeksti()
    { 
        Keyboard.Clear();
        PhysicsObject voitto = new PhysicsObject(Level.Width, Level.Height);
        voitto.X = Level.Center.X;
        voitto.Y = Level.Center.Y;
        voitto.Image = LoadImage("Winner");
        Add(voitto, 1);
        MessageDisplay.Add("Esc to quit the game.");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "quit game");
    }
   
}
