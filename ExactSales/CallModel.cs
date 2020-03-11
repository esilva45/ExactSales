using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceIntegration {
    class CallModel {
        public CallModel() { }

        public CallModel(string url_ligacao, string origem_tel, string destino_tel, string dt_inicio_chamada, string dt_fim_chamada,
            string tempo_conversacao) {
            UrlLigacao = url_ligacao;
            OrigemTel = origem_tel;
            DestinoTel = destino_tel;
            DtInicioChamada = dt_inicio_chamada;
            DtFimChamada = dt_fim_chamada;
            TempoConversacao = tempo_conversacao;
        }

        public string UrlLigacao { get; set; }

        public string OrigemTel { get; set; }

        public string DestinoTel { get; set; }

        public string DtInicioChamada { get; set; }

        public string DtFimChamada { get; set; }

        public string TempoConversacao { get; set; }
    }
}
