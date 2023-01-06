Func<double, double> function = new Func<double, double>((x) => {
    if(x >= 0 && x < 0.1)
        return 0.2;
    else if(x < 0.2)
        return 0.4;
    else if(x < 0.4)
        return 0.6;
    else if(x < 0.8)
        return 0.8;
    else
        return 1;
});
for(int i = 1; i <= 10; i++)
{
    Console.WriteLine(BernsteinPolynome.ToBernsteinPolynome(i, function));
}

class Polynome
{
    public int Times { get; init; }
    public double[] Coefficients { get; set; }
    public Polynome(int n) => (Times, Coefficients) = (n, new double[n+1]);
    public Polynome(int n, double[] coefficents)
    {
        Times = n;
        Coefficients = new double[n+1];
        for(int i = 0; i < n+1 && i < coefficents.Length; i++)
            Coefficients[i] = coefficents[i];
    }

    public static implicit operator Polynome(double value) =>
        new Polynome(0, new double[]{value});
    
    public static implicit operator Func<double, double>(Polynome poly) =>
        new Func<double, double>((double x) => poly[x]);

    public double this[double value]
    {
        get
        {
            double _value = 0;
            for(int i = 0; i <= Times; i++)
                _value += Coefficients[i] * Math.Pow(value, i);
            return _value;
        }
    }

    public static Polynome operator+(Polynome lPoly, Polynome rPoly)
    {
        Polynome result = new Polynome((lPoly.Times < rPoly.Times) ? rPoly.Times : lPoly.Times);
        for(int i = 0; i <= lPoly.Times; i++)
            result.Coefficients[i] += lPoly.Coefficients[i];
        for(int i = 0; i <= rPoly.Times; i++)
            result.Coefficients[i] += rPoly.Coefficients[i];
        return result.Trim();
    }

    public static Polynome operator-(Polynome value)
    {
        Polynome result = new Polynome(value.Times);
        for(int i = 0; i <= value.Times; i++)
            result.Coefficients[i] = -value.Coefficients[i];
        return result;
    }

    public static Polynome operator-(Polynome lPoly, Polynome rPoly) =>
        lPoly + (-rPoly);

    public static Polynome operator*(Polynome lPoly, Polynome rPoly)
    {
        Polynome result = new Polynome(lPoly.Times + rPoly.Times);
        for(int i = 0; i <= lPoly.Times; i++)
        for(int j = 0; j <= rPoly.Times; j++)
        {
            result.Coefficients[i + j] += lPoly.Coefficients[i] * rPoly.Coefficients[j];
        }
        return result.Trim();
    }

    public static Polynome operator*(double factor, Polynome rPoly)
    {
        Polynome result = new Polynome(rPoly.Times);
        rPoly.Coefficients.CopyTo(result.Coefficients, 0);
        for(int i = 0; i <= rPoly.Times; i++)
            result.Coefficients[i] *= factor;
        return result.Trim();
    }

    public static Polynome operator*(Polynome lPoly, double factor) =>
        factor * lPoly;
    
    public static Polynome operator/(Polynome lPoly, double factor) =>
        (1 / factor) * lPoly;

    public Polynome Trim()
    {
        int i;
        for(i = Times; i >= 0; i--)
            if(Coefficients[i] != 0) break;
        Polynome result = new Polynome(i);
        for(int j = 0; j <= i; j++)
            result.Coefficients[j] = Coefficients[j];
        return result;
    }

    public override string ToString()
    {
        string result = String.Empty;
        for(int i = Times; i >= 0; i--)
        {
            if(Coefficients[i] > 0)
                result += $"+{Coefficients[i]}*(x.^{i})";
            else if(Coefficients[i] < 0)
                result += $"{Coefficients[i]}*(x.^{i})";
        }
        return result;
    }
}

class BernsteinPolynome : Polynome
{
    public BernsteinPolynome(int n, int i) : base(n)
    {
        for(int k = 0; k <= n-i; k++)
        {
            int c = 1;
            for(int s = 0; s < i + k; s++)
                c *= n-s;
            for(int s = 1; s <= i; s++)
                c /= s;
            for(int s = 1; s <= k; s++)
                c /= s;
            Coefficients[i+k] = k % 2 == 0 ? c : -c;
        }        
    }

    public static Polynome ToBernsteinPolynome(int n, Func<double, double> function)
    {
        Polynome result = new Polynome(n);
        for(int i = 0; i <= n; i++)
        {
            result += function(1.0 / n * i) * new BernsteinPolynome(n, i);
        }
        return result;
    }
}