﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Remoting;
using Common.HardwareSupport;
using Common.MacroProgramming;
using LightningGauges.Renderers.F16;
using System.IO;
using log4net;

namespace SimLinkup.HardwareSupport.Simtek
{
    //Simtek 10-0294 F-16 Simulated Fuel Quantity Indicator
    public class Simtek100294HardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(Simtek100294HardwareSupportModule));
        private readonly IFuelQuantityIndicator _renderer = new FuelQuantityIndicator();
        private AnalogSignal _aftLeftFuelInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _aftLeftFuelInputSignalChangedEventHandler;
        private AnalogSignal _aftLeftOutputSignal;
        private AnalogSignal _counterOutputSignal;
        private AnalogSignal _foreRightFuelInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _foreRightFuelInputSignalChangedEventHandler;
        private AnalogSignal _foreRightOutputSignal;
        private const uint DEFAULT_MAX_POUNDS_TOTAL_FUEL = 9900;
        private uint _maxPoundsTotalFuel = DEFAULT_MAX_POUNDS_TOTAL_FUEL;
        private bool _isDisposed;
        private AnalogSignal _totalFuelInputSignal;
        private AnalogSignal.AnalogSignalChangedEventHandler _totalFuelInputSignalChangedEventHandler;

        private Simtek100294HardwareSupportModule()
        {
            LoadConfig();
            CreateInputSignals();
            CreateOutputSignals();
            CreateInputEventHandlers();
            RegisterForInputEvents();
        }

        public override AnalogSignal[] AnalogInputs => new[]
            {_totalFuelInputSignal, _foreRightFuelInputSignal, _aftLeftFuelInputSignal};

        public override AnalogSignal[] AnalogOutputs => new[]
            {_foreRightOutputSignal, _aftLeftOutputSignal, _counterOutputSignal};

        public override DigitalSignal[] DigitalInputs => null;

        public override DigitalSignal[] DigitalOutputs => null;

        public override string FriendlyName => "Simtek P/N 10-0294 - Indicator - Simulated Fuel Qty";

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Simtek100294HardwareSupportModule()
        {
            Dispose();
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>
            {
                new Simtek100294HardwareSupportModule()
            };

            return toReturn.ToArray();
        }

        public override void Render(Graphics g, Rectangle destinationRectangle)
        {
            _renderer.InstrumentState.AftLeftFuelQuantityPounds = (float) _aftLeftFuelInputSignal.State;
            _renderer.InstrumentState.ForeRightFuelQuantityPounds = (float) _foreRightFuelInputSignal.State;
            _renderer.InstrumentState.TotalFuelQuantityPounds = (float) _totalFuelInputSignal.State;
            _renderer.Render(g, destinationRectangle);
        }

        private void AbandonInputEventHandlers()
        {
            _totalFuelInputSignalChangedEventHandler = null;
            _foreRightFuelInputSignalChangedEventHandler = null;
            _aftLeftFuelInputSignalChangedEventHandler = null;
        }
        private void LoadConfig()
        {
            try
            {
                var hsmConfigFilePath = Path.Combine(Util.CurrentMappingProfileDirectory, "Simtek100294HardwareSupportModule.config");
                var hsmConfig = Simtek100294HardwareSupportModuleConfig.Load(hsmConfigFilePath);
                _maxPoundsTotalFuel = hsmConfig.MaxPoundsTotalFuel.HasValue ? hsmConfig.MaxPoundsTotalFuel.Value : DEFAULT_MAX_POUNDS_TOTAL_FUEL;
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
        }
    
        private AnalogSignal CreateAftLeftFuelInputSignal()
        {
            var thisSignal = new AnalogSignal
            {
                Category = "Inputs",
                CollectionName = "Analog Inputs",
                FriendlyName = "A/L Fuel",
                Id = "100294_AftAndLeft_Fuel_From_Sim",
                Index = 0,
                Source = this,
                SourceFriendlyName = FriendlyName,
                SourceAddress = null,
                State = 0,
                MinValue = 0,
                MaxValue = 42000
            };
            return thisSignal;
        }

        private AnalogSignal CreateAftLeftOutputSignal()
        {
            var thisSignal = new AnalogSignal
            {
                Category = "Outputs",
                CollectionName = "Analog Outputs",
                FriendlyName = "A/L",
                Id = "100294_AL_To_Instrument",
                Index = 0,
                Source = this,
                SourceFriendlyName = FriendlyName,
                SourceAddress = null,
                State = -10.00, //volts
                IsVoltage = true,
                MinValue = -10,
                MaxValue = 10
            };
            return thisSignal;
        }

        private AnalogSignal CreateCounterOutputSignal()
        {
            var thisSignal = new AnalogSignal
            {
                Category = "Outputs",
                CollectionName = "Analog Outputs",
                FriendlyName = nameof(Counter),
                Id = "100294_Counter_To_Instrument",
                Index = 0,
                Source = this,
                SourceFriendlyName = FriendlyName,
                SourceAddress = null,
                State = -10.00, //volts
                IsVoltage = true,
                MinValue = -10,
                MaxValue = 10
            };
            return thisSignal;
        }

        private AnalogSignal CreateForeRightFuelInputSignal()
        {
            var thisSignal = new AnalogSignal
            {
                Category = "Inputs",
                CollectionName = "Analog Inputs",
                FriendlyName = "F/R Fuel",
                Id = "100294_ForeAndRight_Fuel_From_Sim",
                Index = 0,
                Source = this,
                SourceFriendlyName = FriendlyName,
                SourceAddress = null,
                State = 0,
                MinValue = 0,
                MaxValue = 42000
            };
            return thisSignal;
        }

        private AnalogSignal CreateForeRightOutputSignal()
        {
            var thisSignal = new AnalogSignal
            {
                Category = "Outputs",
                CollectionName = "Analog Outputs",
                FriendlyName = "F/R",
                Id = "100294_FR_To_Instrument",
                Index = 0,
                Source = this,
                SourceFriendlyName = FriendlyName,
                SourceAddress = null,
                State = -10.00, //volts
                IsVoltage = true,
                MinValue = -10,
                MaxValue = 10
            };
            return thisSignal;
        }

        private void CreateInputEventHandlers()
        {
            _totalFuelInputSignalChangedEventHandler =
                fuel_InputSignalChanged;
            _aftLeftFuelInputSignalChangedEventHandler =
                fuel_InputSignalChanged;
            _foreRightFuelInputSignalChangedEventHandler =
                fuel_InputSignalChanged;
        }

        private void CreateInputSignals()
        {
            _totalFuelInputSignal = CreateTotalFuelInputSignal();
            _aftLeftFuelInputSignal = CreateAftLeftFuelInputSignal();
            _foreRightFuelInputSignal = CreateForeRightFuelInputSignal();
        }


        private void CreateOutputSignals()
        {
            _foreRightOutputSignal = CreateForeRightOutputSignal();
            _aftLeftOutputSignal = CreateAftLeftOutputSignal();
            _counterOutputSignal = CreateCounterOutputSignal();
        }

        private AnalogSignal CreateTotalFuelInputSignal()
        {
            var thisSignal = new AnalogSignal
            {
                Category = "Inputs",
                CollectionName = "Analog Inputs",
                FriendlyName = "Total Fuel (Pounds)",
                Id = "100294_Total_Fuel_From_Sim",
                Index = 0,
                Source = this,
                SourceFriendlyName = FriendlyName,
                SourceAddress = null,
                State = 0,
                MinValue = 0,
                MaxValue = _maxPoundsTotalFuel
            };

            return thisSignal;
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    UnregisterForInputEvents();
                    AbandonInputEventHandlers();
                    Common.Util.DisposeObject(_renderer);
                }
            }
            _isDisposed = true;
        }

        private void fuel_InputSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            UpdateOutputValues();
        }

        private void RegisterForInputEvents()
        {
            if (_totalFuelInputSignal != null)
            {
                _totalFuelInputSignal.SignalChanged += _totalFuelInputSignalChangedEventHandler;
            }
            if (_aftLeftFuelInputSignal != null)
            {
                _aftLeftFuelInputSignal.SignalChanged += _aftLeftFuelInputSignalChangedEventHandler;
            }
            if (_foreRightFuelInputSignal != null)
            {
                _foreRightFuelInputSignal.SignalChanged += _foreRightFuelInputSignalChangedEventHandler;
            }
        }

        private void UnregisterForInputEvents()
        {
            if (_totalFuelInputSignalChangedEventHandler != null && _totalFuelInputSignal != null)
            {
                try
                {
                    _totalFuelInputSignal.SignalChanged -= _totalFuelInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }


            if (_aftLeftFuelInputSignalChangedEventHandler != null && _aftLeftFuelInputSignal != null)
            {
                try
                {
                    _aftLeftFuelInputSignal.SignalChanged -= _aftLeftFuelInputSignalChangedEventHandler;
                }
                catch (RemotingException)
                {
                }
            }
            if (_foreRightFuelInputSignalChangedEventHandler == null || _foreRightFuelInputSignal == null) return;
            try
            {
                _foreRightFuelInputSignal.SignalChanged -= _foreRightFuelInputSignalChangedEventHandler;
            }
            catch (RemotingException)
            {
            }
        }

        private void UpdateOutputValues()
        {
            if (_foreRightOutputSignal != null)
            {
                _foreRightOutputSignal.State = _foreRightFuelInputSignal.State / 100.00 / 42.00 * 20.00 - 10.00;
            }
            if (_aftLeftOutputSignal != null)
            {
                _aftLeftOutputSignal.State = _aftLeftFuelInputSignal.State / 100.00 / 42.00 * 20.00 - 10.00;
            }
            if (_counterOutputSignal != null)
            {
                _counterOutputSignal.State = (_totalFuelInputSignal.State / _maxPoundsTotalFuel) * 20.00 - 10.00;
            }
        }
    }
}