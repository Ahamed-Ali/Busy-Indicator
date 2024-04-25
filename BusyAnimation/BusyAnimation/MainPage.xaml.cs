using Microsoft.Maui.Animations;

namespace BusyAnimation
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

    
    }
    public class BusyIndicator : GraphicsView, IDrawable
    {
        private BusyIndicatorAnimation? busyIndicatorAnimation;

        private IAnimationManager? animationManager;
        public BusyIndicator()
        {
            Drawable = this;
        }

        public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(nameof(IsRunning), typeof(bool), typeof(BusyIndicator), false, BindingMode.TwoWay, null, IsRunningPropertyChanged);

        private static void IsRunningPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {

            BusyIndicator busyIndicator = bindable as BusyIndicator;
            busyIndicator.IsRunning = (bool)newValue;

            if (busyIndicator.busyIndicatorAnimation != null)
            {
                busyIndicator.busyIndicatorAnimation.RunAnimation((bool)newValue);
                busyIndicator.Invalidate();
            }

        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            animationManager = Handler.MauiContext.Services.GetRequiredService<IAnimationManager>();
            this.SetAnimationType();
        }

        private void SetAnimationType()
        {
            if (this.animationManager == null)
                return;


            this.busyIndicatorAnimation = new SingleCircleBusyIndictorAnimation(this.animationManager);

            if (this.busyIndicatorAnimation != null)
            {
                this.busyIndicatorAnimation.Color = Colors.Red;
                this.busyIndicatorAnimation.RunAnimation(this.IsRunning);
                this.busyIndicatorAnimation.AnimationDuration = 0.1;
                this.busyIndicatorAnimation.sizeFactor = 2;
                this.Invalidate();
            }
        }

        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }

        }
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {


            if (IsRunning)
            {

                canvas.SetFillPaint(new SolidColorBrush(Colors.Yellow), dirtyRect);
                canvas.FillRectangle(dirtyRect);


                if (busyIndicatorAnimation != null)
                {
                    busyIndicatorAnimation.DrawAnimation(this, canvas);


                }
            }
        }
    }

    internal abstract class BusyIndicatorAnimation : Microsoft.Maui.Animations.Animation
    {
        #region Fields

        internal GraphicsView? drawableView;

        double secondsSinceLastUpdate;

        private double actualDuration = 0;

        internal double sizeFactor = 0.5;

        internal Rect actualRect = new();

        internal double defaultHeight = 0;

        internal double defaultWidth = 0;

        private double defaultDuration = 100;

        private double animationduration = 0.5;

        private Color color = Color.FromArgb("#FF512BD4");

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        internal double DefaultDuration
        {
            get { return this.defaultDuration; }
            set
            {
                this.defaultDuration = value;
                this.SetActualDuration();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        internal double AnimationDuration
        {
            get
            {
                return animationduration;
            }
            set
            {
                this.animationduration = value;

                if (animationduration > 1)
                    animationduration = 1;

                this.SetActualDuration();
            }
        }

        internal Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        #endregion;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BusyIndicatorAnimation"/> class.
        /// </summary>
        /// /// <param name="animationManagerValue"></param>
        public BusyIndicatorAnimation(IAnimationManager animationManagerValue)
        {
            this.animationManger = animationManagerValue;
            this.Easing = Microsoft.Maui.Easing.SinIn;
            this.Repeats = true;

            this.SetActualDuration();
        }

        #endregion

        #region Virtual Methods

        protected virtual void OnDrawAnimation(GraphicsView view, ICanvas canvas)
        {

        }

        protected virtual void OnUpdateAnimation()
        {

        }

        #endregion

        #region Override Methods

        protected override void OnTick(double millisecondsSinceLastUpdate)
        {
            base.OnTick(millisecondsSinceLastUpdate);

            this.secondsSinceLastUpdate += millisecondsSinceLastUpdate;

            if (this.secondsSinceLastUpdate > this.actualDuration)
            {
                this.UpdateActualRect();
                this.OnUpdateAnimation();

                this.secondsSinceLastUpdate = 0;
            }
        }

        #endregion

        #region Methods

        internal void UpdateActualRect()
        {
            if (drawableView == null)
                return;

            double centerX1 = this.drawableView.Width / 2;
            double centerY1 = this.drawableView.Height / 2;

            double centerX2 = this.defaultWidth / 2;
            double centerY2 = this.defaultHeight / 2;

            centerX2 = centerX2 * sizeFactor;
            centerY2 = centerY2 * sizeFactor;

            this.actualRect.X = centerX1 - centerX2;
            this.actualRect.Y = centerY1 - centerY2;
            this.actualRect.Width = this.defaultWidth * this.sizeFactor;
            this.actualRect.Height = this.defaultHeight * this.sizeFactor;

        }

        internal void DrawAnimation(GraphicsView view, ICanvas canvas)
        {
            if (this.drawableView == null)
            {
                this.drawableView = view;
            }

            this.OnDrawAnimation(view, canvas);
        }

        private void SetActualDuration()
        {
            this.actualDuration = this.AnimationDuration * this.DefaultDuration;
        }

        internal void RunAnimation(bool canStart)
        {
            this.HasFinished = !canStart;

            if (canStart)
                this.Resume();
            else
                this.Pause();
        }

        #endregion
    }

    internal class SingleCircleBusyIndictorAnimation : BusyIndicatorAnimation
    {
        #region Fields

        private readonly float itemNumber = 8;

        private readonly float lineSize = 12;

        private float startAngle = 0;

        private float endAngle = -40;

        private float count = 10;

        #endregion

        #region Constructor
        public SingleCircleBusyIndictorAnimation(IAnimationManager animationManagerValue) : base(animationManagerValue)
        {
            this.defaultWidth = 75;
            this.defaultHeight = 75;
        }
        #endregion

        #region Override Mehtods
        protected override void OnDrawAnimation(GraphicsView view, ICanvas canvas)
        {
            base.OnDrawAnimation(view, canvas);

            canvas.StrokeSize = this.lineSize * (float)this.sizeFactor;
            canvas.StrokeColor = this.Color;
            canvas.StrokeLineCap = LineCap.Butt;

            for (int i = 0; i < itemNumber; i++)
            {
                canvas.DrawArc(actualRect, startAngle - count, endAngle - count, true, false);
                startAngle = endAngle - 5;
                endAngle = startAngle - 40;
            }

        }

        protected override void OnUpdateAnimation()
        {
            base.OnUpdateAnimation();

            count += 1;
            if (count > 360)
            {
                count = 1;
            }

            this.drawableView?.Invalidate();
        }
        #endregion
    }


}
