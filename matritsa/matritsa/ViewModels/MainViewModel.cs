
using matritsa.Properties;
using matritsa.Util;
using Matritsa.PDFGenerator.Data;
using Matritsa.PDFGenerator;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using Avalonia.Threading;

namespace matritsa.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    public string? CSVFile { get; set; } = null;


    public float? PageWidth {
        get => _ignPgSz ? null : _pageW;
        set => this.RaiseAndSetSaveAffectingIfChanged(ref _pageW, value);  
    }
    public float? PageHeight {
        get => _ignPgSz ? null : _pageH;
        set => this.RaiseAndSetSaveAffectingIfChanged(ref _pageH, value);
    }
    public float? MatrixSize {
        get => _matSz;
        set => this.RaiseAndSetSaveAffectingIfChanged(ref _matSz, value);
    }
    public float? MatrixFrameWidth {
        get => _matFW;
        set {
            this.RaiseAndSetSaveAffectingIfChanged(ref _matFW, value);
            if (_ignPgSz) {
                this.RaisePropertyChanged(nameof(PageWidth));
            }
        }
    }
    public float? MatrixFrameHeight {
        get => _matFH;
        set {
            this.RaiseAndSetSaveAffectingIfChanged(ref _matFH, value);
            if (_ignPgSz) {
                this.RaisePropertyChanged(nameof(PageHeight));
            }
        }
    }
    public bool IgnorePageSize {
        get => _ignPgSz;
        set {
            this.RaiseAndSetIfChanged(ref _ignPgSz, value);
            this.RaisePropertyChanged(nameof(SaveBlockedReason));
            this.RaisePropertyChanged(nameof(CodesPerPage));
            this.RaisePropertyChanged(nameof(PageWidth));
            this.RaisePropertyChanged(nameof(PageHeight));
        }
    }

    private void RaiseAndSetSaveAffectingIfChanged<T>(ref T? backing, T? newValue) {
        var oldValue = backing;
        this.RaiseAndSetIfChanged(ref backing, newValue);
        this.RaisePropertyChanged(nameof(SaveBlockedReason));
        this.RaisePropertyChanged(nameof(CodesPerPage));
    }

    public string? SaveBlockedReason {
        get {
            if (CSVFile == null) return Resources.generateBlockedReasonList;
            else if (_matSz == 0 || _matSz == null) return Resources.generateBlockedReasonMtrxZero;
            else if (_matFH < _matSz || _matFW < _matSz) return Resources.generateBlockedReasonFrameSmall;

            else if (!_ignPgSz && (_pageW == 0 || _pageH == 0)) return Resources.generateBlockedReasonPageZero;
            else if (!_ignPgSz && (_pageW == null || _pageH == null)) return Resources.generateBlockedReasonPageZero;
            else if (!_ignPgSz && (_matSz > _pageW || _matSz > _pageH)) return Resources.generateBlockedReasonMtrxLarge;
            else if (!_ignPgSz && (_matFH > _pageH || _matFW > _pageW)) return Resources.generateBlockedReasonFrameLarge;

            else return null;
        }
    }

    public PluralSet CodesPluralSet = new(
        Resources.codesPerPage_One,
        Resources.codesPerPage_Other,
        Resources.codesPerPage_Few,
        Resources._util_pluralForm
    );

    public string CodesPerPage {
        get {
            if (_ignPgSz) return "";
            var area = (int)((_pageW /_matFW) * (_pageH/_matFH) ?? 0);
            return string.Format(PluralEngine.GetPluralForm(area, CodesPluralSet), area);
        }
    }

    public float? GenerationProgress {
        get => _genPs;
        set => this.RaiseAndSetIfChanged(ref _genPs, value);
    }
    public bool ShowGenerationProgress {
        get => _showGen;
        set => this.RaiseAndSetIfChanged(ref _showGen, value);
    }

    private float? _pageW = 297;
    private float? _pageH = 210;
    private float? _matSz = 10;
    private float? _matFW = 15;
    private float? _matFH = 15;
    private float? _genPs = 0;
    private bool _showGen = false;
    private bool _ignPgSz = false;

    internal void SetCSVFile(string v) {
        CSVFile = v;
        this.RaisePropertyChanged(nameof(CSVFile));
        this.RaisePropertyChanged(nameof(SaveBlockedReason));
    }

    private PDFGenerator generator = new(new PDFOptions(PaperType.A4, new Dimensions<float>(15, 15, MeasurementUnit.Millimeter), 10));
    private CancellationTokenSource? genTaskCancel = null;

    internal void GeneratePreview(CancellationToken token) {
        var layout = generator.GeneratePrintPreviewData(
            (mat, progress) => {
                GenerationProgress = progress * 100;
            },
            token
        );
        //pdf.Save(path);
        //Utils.OpenUrl(path);
    }

    internal void StartPrintPreview() {
        // сгенерируем название временного файла
        //var filename = "matrigen_preview_" + new Random().Next().ToString() + ".pdf";
        //var path = Path.Join(Path.GetTempPath(), filename);
        // создаем источник токена
        genTaskCancel = new CancellationTokenSource();
        // записываем параметры
        generator.SetOptions(new PDFOptions(
            new PaperType(
                new Dimensions<float>(
                    PageWidth != null ? (float)PageWidth : 0F,
                    PageHeight != null ? (float)PageHeight : 0F,
                    MeasurementUnit.Millimeter
                ),
                10
            ),
            new Dimensions<float>(
                (float)MatrixFrameWidth,
                (float)MatrixFrameHeight,
                MeasurementUnit.Millimeter
            ),
            (float)MatrixSize
        ));
        // создаем токен и фоновую задачу
        var token = genTaskCancel.Token;
        var genTask = new Task(() => GeneratePreview(token), token);
        // запускаем задачу
        genTask.Start();
    }

    internal void StartGeneration(Uri outPath) {
        genTaskCancel = new CancellationTokenSource();
        // записываем параметры
        generator.SetOptions(new PDFOptions(
            new PaperType(
                new Dimensions<float>(
                    PageWidth != null ? (float)PageWidth : 0F,
                    PageHeight != null ? (float)PageHeight : 0F,
                    MeasurementUnit.Millimeter
                ),
                10
            ),
            new Dimensions<float>(
                (float)MatrixFrameWidth,
                (float)MatrixFrameHeight,
                MeasurementUnit.Millimeter
            ),
            (float)MatrixSize
        ));

        // создаем токен и фоновую задачу
        var token = genTaskCancel.Token;
        var genTask = new Task(() => GeneratePDF(
            HttpUtility.UrlDecode(CSVFile.Replace("file:///", "")),
            token,
            IgnorePageSize,
            outPath
        ), token);
        // запускаем задачу
        genTask.Start();
    }

    private void GeneratePDF(string url, CancellationToken token, bool ignorePageSize, Uri outPath) {
        using var stream = File.Open(url, FileMode.Open);
        using StreamReader reader = new(stream);

        string fileContents = reader.ReadToEnd();

        stream.Close();

        ShowGenerationProgress = true;
        try {
            var pdf = generator.Generate(
                fileContents.Split("\n", StringSplitOptions.RemoveEmptyEntries),
                string.Format(Resources.pdfTitle, Path.GetFileNameWithoutExtension(url)),
                (mat, progress) => {
                    GenerationProgress = progress * 100;
                },
                token,
                ignorePageSize
            );
            if (outPath != null) {
                pdf.Save(HttpUtility.UrlDecode(outPath.LocalPath));
                Utils.OpenUrl(HttpUtility.UrlDecode(outPath.OriginalString));
            }
            Debug.WriteLine("Generated");
        }
        catch (OperationCanceledException) {
            Debug.WriteLine("Operation was canceled!");
        }
        ShowGenerationProgress = false;
    }

    internal void CancelGeneration() {
        genTaskCancel?.Cancel();
        ShowGenerationProgress = false;
    }
}
