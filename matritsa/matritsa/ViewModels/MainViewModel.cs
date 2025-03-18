
using matritsa.Properties;
using matritsa.Util;
using ReactiveUI;
using System;
using System.Diagnostics;

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
            /*if (_ignPgSz) {
                this.RaisePropertyChanged(nameof(PageWidth));
            }*/
        }
    }
    public float? MatrixFrameHeight {
        get => _matFH;
        set {
            this.RaiseAndSetSaveAffectingIfChanged(ref _matFH, value);
            /*if (_ignPgSz) {
                this.RaisePropertyChanged(nameof(PageHeight));
            }*/
        }
    }
    public bool IgnorePageSize {
        get => _ignPgSz;
        set {
            this.RaiseAndSetSaveAffectingIfChanged(ref _ignPgSz, value);
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

    private float? _pageW = 297;
    private float? _pageH = 210;
    private float? _matSz = 10;
    private float? _matFW = 15;
    private float? _matFH = 15;
    private bool _ignPgSz = false;

    internal void SetCSVFile(string v) {
        CSVFile = v;
        this.RaisePropertyChanged(nameof(CSVFile));
        this.RaisePropertyChanged(nameof(SaveBlockedReason));
    }
}
