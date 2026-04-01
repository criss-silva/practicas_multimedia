using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace capybara;

internal class Enemigo : ScaledSprite
{
    private float _speed = 1.3f; 
    private Vector2 _velocity;
    private Vector2 _posicionInicial;
    private int _tileSize = 60;

    private int _totalFrames;
    private int _frameActual;
    private float _timerAnimacion;
    private float _tiempoPorFrame = 0.1f;
    private int _anchoFrame;
    private int _altoFrame;

    public BoxCollider Collider { get; private set; }

    public Enemigo(Texture2D texture, Vector2 position, float scale, int totalFrames) 
        : base(texture, position, scale)
    {
        this.position = position;
        this._posicionInicial = position;
        this._totalFrames = totalFrames;
        this._anchoFrame = texture.Width / _totalFrames;
        this._altoFrame = texture.Height;

        //collider para el enemigo
        int colAncho = (int)(_anchoFrame * scale) - 15;
        int colAlto = (int)(_altoFrame * scale) - 15;
        Collider = new BoxCollider(position, colAncho, colAlto, false);
    }

    public void Update(GameTime gameTime, MovedSprite jugador, Dictionary<Vector2, int> tilemap)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        //oobtenemos el suelo para que este siempre a ua altura fija del suelo, asi el enemigo siempre estara volando a la misma altura
        float ySuelo = ObtenerYSueloDebajo(tilemap);
        if (ySuelo >= 2000f) ySuelo = 600f; 

        // pra perseguir al jugador
        float alturaObjetivo = jugador.position.Y;

        
        float limiteVueloBajo = ySuelo - (Collider.Height / 2f) - 5f;
        if (alturaObjetivo > limiteVueloBajo) 
        {
            alturaObjetivo = limiteVueloBajo;
        }

       // lógica de movimiento hacia el jugador y animacion
        float dirX = (jugador.position.X < this.position.X) ? -1 : 1;
        _velocity.X = dirX * _speed;
        efecto = (dirX == -1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        float diferenciaY = alturaObjetivo - this.position.Y;
        _velocity.Y = MathHelper.Clamp(diferenciaY * 0.08f, -3.2f, 3.2f);
        _timerAnimacion += dt;
        if (_timerAnimacion >= _tiempoPorFrame)
        {
            _frameActual = (_frameActual + 1) % _totalFrames;
            _timerAnimacion = 0;
        }

        MoverConFisicas(tilemap);
        DetectarDano(jugador);
    }

// movimiento con colisiones con el tilemap para quue no se atasque en las esquians
    private void MoverConFisicas(Dictionary<Vector2, int> tilemap)
    {
       
        float oldX = position.X;
        position.X += _velocity.X;
        SincronizarCollider();
        if (TocandoBloque(tilemap)) { position.X = oldX; _velocity.X = 0; }
        float oldY = position.Y;
        position.Y += _velocity.Y;
        SincronizarCollider();
        if (TocandoBloque(tilemap)) { position.Y = oldY; _velocity.Y = 0; }
    }

// lógica de daño al jugador
    private void DetectarDano(MovedSprite jugador)
    {
       
        Rectangle hurtboxM = GetRectReducido(0.5f);
    
        Rectangle hurtboxJ = new Rectangle(
            (int)jugador.Collider.Position.X, 
            (int)jugador.Collider.Position.Y, 
            jugador.Collider.Width, 
            jugador.Collider.Height
        );
        hurtboxJ.Inflate(-15, -10);

        if (hurtboxM.Intersects(hurtboxJ))
        {
            VidaManager.PerderVida();
            ResetearPosicion();
        }
    }
// para que cuando pierda una vida vuelva a la posición inicial, y para que no siga con inercia
    public void ResetearPosicion()
    {
        this.position = _posicionInicial;
        this._velocity = Vector2.Zero;
        SincronizarCollider();
    }
// para mantener el collider sincronizado con la posición del enemigo
    private void SincronizarCollider()
    {
        Collider.Position = new Vector2(position.X - Collider.Width / 2f, position.Y - Collider.Height / 2f);
    }

// rectangulo de la colision con el jugador un poco más peque
    private Rectangle GetRectReducido(float porcentaje)
    {
        int w = (int)(_anchoFrame * scale * porcentaje);
        int h = (int)(_altoFrame * scale * porcentaje);
        return new Rectangle((int)position.X - w / 2, (int)position.Y - h / 2, w, h);
    }

//colisiones con el tilemap
    private bool TocandoBloque(Dictionary<Vector2, int> tilemap)
    {
        Rectangle rEnemigo = new Rectangle((int)Collider.Position.X, (int)Collider.Position.Y, Collider.Width, Collider.Height);
        
        foreach (var tile in tilemap)
        {
            if (tile.Value != 18 && tile.Value != 99) 
            {
                Rectangle rBloque = new Rectangle((int)tile.Key.X * _tileSize, (int)tile.Key.Y * _tileSize, _tileSize, _tileSize);
                if (rEnemigo.Intersects(rBloque)) return true;
            }
        }
        return false;
    }

    private float ObtenerYSueloDebajo(Dictionary<Vector2, int> tilemap) //para obtener el suelo de debajo del enemigo más cercano
    {
        int colX = (int)Math.Floor(this.position.X / _tileSize);
        float yEncontrada = 3000f; 
        
        foreach (var tile in tilemap)
        {
            if ((int)tile.Key.X == colX && tile.Value != 18 && tile.Value != 99)
            {
                float yPixel = tile.Key.Y * _tileSize;
                if (yPixel > this.position.Y - 20 && yPixel < yEncontrada) 
                {
                    yEncontrada = yPixel;
                }
            }
        }
        return yEncontrada;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle fuente = new Rectangle(_frameActual * _anchoFrame, 0, _anchoFrame, _altoFrame);
        Vector2 origen = new Vector2(_anchoFrame / 2f, _altoFrame / 2f);
        spriteBatch.Draw(texture, position, fuente, Color.White, 0f, origen, scale, efecto, 0f);
    }
}