using System.ServiceProcess;
using System.Threading;

namespace ServiceIntegration {
    public partial class Service1 : ServiceBase {
        private Timer _timer;

        public Service1() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            _timer = new Timer(ProcessorManager, null, 0, 60000);
        }

        protected override void OnStop() { }

        private void ProcessorManager(object state) {
            SendMessage.Message();
        }
    }
}
