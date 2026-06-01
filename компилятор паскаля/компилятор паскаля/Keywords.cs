using System.Collections.Generic;


class Keywords
{
    static Dictionary<byte, Dictionary<string, byte>> kw = new Dictionary<byte, Dictionary<string, byte>>();
    public Dictionary<byte, Dictionary<string, byte>> Kw
    {
        get { return kw; }
    }
    public Keywords()
    {
        Dictionary<string, byte> tmp = new Dictionary<string, byte>();
        tmp["do"] = LexicalAnalyzer.DoSy;
        tmp["if"] = LexicalAnalyzer.IfSy;
        tmp["in"] = LexicalAnalyzer.InSy;
        tmp["of"] = LexicalAnalyzer.OfSy;
        tmp["or"] = LexicalAnalyzer.OrSy;
        tmp["to"] = LexicalAnalyzer.ToSy;
        kw[2] = tmp;
        tmp = new Dictionary<string, byte>();
        tmp["end"] = LexicalAnalyzer.EndSy;
        tmp["var"] = LexicalAnalyzer.VarSy;
        tmp["div"] = LexicalAnalyzer.DivSy;
        tmp["and"] = LexicalAnalyzer.AndSy;
        tmp["not"] = LexicalAnalyzer.NotSy;
        tmp["for"] = LexicalAnalyzer.ForSy;
        tmp["mod"] = LexicalAnalyzer.ModSy;
        tmp["nil"] = LexicalAnalyzer.NilSy;
        tmp["set"] = LexicalAnalyzer.SetSy;
        kw[3] = tmp;
        tmp = new Dictionary<string, byte>();
        tmp["then"] = LexicalAnalyzer.ThenSy;
        tmp["else"] = LexicalAnalyzer.ElseSy;
        tmp["case"] = LexicalAnalyzer.CaseSy;
        tmp["file"] = LexicalAnalyzer.FileSy;
        tmp["goto"] = LexicalAnalyzer.GotoSy;
        tmp["type"] = LexicalAnalyzer.TypeSy;
        tmp["with"] = LexicalAnalyzer.WithSy;
        kw[4] = tmp;
        tmp = new Dictionary<string, byte>();
        tmp["begin"] = LexicalAnalyzer.BeginSy;
        tmp["while"] = LexicalAnalyzer.WhileSy;
        tmp["array"] = LexicalAnalyzer.ArraySy;
        tmp["const"] = LexicalAnalyzer.ConstSy;
        tmp["label"] = LexicalAnalyzer.LabelSy;
        tmp["until"] = LexicalAnalyzer.UntilSy;
        kw[5] = tmp;
        tmp = new Dictionary<string, byte>();
        tmp["downto"] = LexicalAnalyzer.DowntoSy;
        tmp["packed"] = LexicalAnalyzer.PackedSy;
        tmp["record"] = LexicalAnalyzer.RecordSy;
        tmp["repeat"] = LexicalAnalyzer.RepeatSy;
        kw[6] = tmp;
        tmp = new Dictionary<string, byte>();
        tmp["program"] = LexicalAnalyzer.ProgramSy;
        kw[7] = tmp;
        tmp = new Dictionary<string, byte>();
        tmp["function"] = LexicalAnalyzer.FunctionSy;
        kw[8] = tmp;
        tmp = new Dictionary<string, byte>();
        tmp["procedure"] = LexicalAnalyzer.ProcedurenSy;
        kw[9] = tmp;
    }
}
