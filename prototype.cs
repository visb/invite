
class ConviteModel {
  public id;
  public appId;
  public solicitanteId;
  public doadorId;
  public dataSolicitacao;
  public dataDoacao;
  public dataConfirmacao;
}

class UsuarioModel {
  public id;
  public nome;
  public email;
}

class ConviteService {

  void function solicitar(int appId) {
    var solicitacao = new ConviteModel;
    solicitacao.solicitanteId = CurrentUser.id;
    solicitacao.appId = appId;
    solicitacao.dataSolicitacao = 'Y-m-d H:i:s';
    solicitacao.save();
  }

  void function doar(int SolicitacaoId) {
    var solicitacao = ConviteModel.get(SolicitacaoId);
    solicitacao.doadorId = CurrentUser.id;
    solicitacao.status = 'doado';
    solicitacao.dataDoacao = 'Y-m-d H:i:s';
    solicitacao.save();

    Notificacoes.doacaoRecebida(SolicitacaoId);
  }

  void function confirmar(SolicitacaoId) {
    var solicitacao = ConviteModel.get(SolicitacaoId);

    if (solicitacao.solicitanteId != CurrentUser.id) {
      throw new Exception('Essa solicitação não é sua');
    }

    solicitacao.status = 'confirmado';
    solicitacao.save();

    Notificacoes.doacaoConfirmada(solicitacao.id);
  }

}

class Notificacoes {

  void function doacaoConfirmada(int SolicitacaoId) {
    var solicitacao = ConviteModel.get(SolicitacaoId);
    var solicitante = UsuarioModel.get(solicitacao.solicitanteId);
    var doador = UsuarioModel.get(solicitacao.doadorId);
    var app = AppModel.get(solicitacao.appId);

    var body = new MailTemplate('doacao.confirmada', [
      :solicitacao
      :solicitante
      :doador
      :app
    ]);

    var to = [doador.email, doador.nome];
    var subject = 'Sua doação foi confirmada';
    var mail = MailSender(to, subject, body.toString());
    mail.send();
  }

  void function doacaoRecebida(int SolicitacaoId) {
    var solicitacao = ConviteModel.get(SolicitacaoId);
    var solicitante = UsuarioModel.get(solicitacao.solicitanteId);
    var doador = UsuarioModel.get(solicitacao.doadorId);
    var app = AppModel.get(solicitacao.appId);

    var body = new MailTemplate('doacao.recebida', [
      :solicitacao
      :solicitante
      :doador
      :app
    ]);
    var to = [solicitante.email, solicitante.nome];
    var subject = 'Você recebeu uma doaçao';
    var mail = MailSender(to, subject, body.toString());
    mail.send();
  }

}


// rest controllers
class ConvitesController {
  Response function postSolicitar() {
    var appId = Request.postParam('app_id');
    ConviteService.solicitar(appId);
    return Response(status: 200);
  }

  Response function postDoar() {
    var solicitacaoId = Request.postParam('solicitacao_id');
    ConviteService.doar(solicitacaoId);
    return Response(status: 200);
  }

  Response function postConfirmar() {
    var solicitacaoId = Request.postParam('solicitacao_id');
    ConviteService.solicitar(solicitacaoId);
    return Response(status: 200);
  }

  Response function getSolicitacao() {
    var solicitacaoId = Request.getParam('solicitacao_id');
    var solicitacao = ConviteModel.get(solicitacaoId);
    solicitacao.solicitante = UsuarioModel.get(solicitacao.solicitanteId);
    return Response(200, solicitacao);
  }
}

class UsuariosController {
  Response function getProfile() {
    var usuario = UsuarioModel.get(CurrentUser.id);
    return Response(200, usuario);
  }

  Response function getConfiguracoes() {
    var usuario = UsuarioModel.get(CurrentUser.id);
    return Response(200, usuario);
  }

  Response function postConfiguracoes() {
    UsuarioForm.setData(Request.postParams());
    UsuarioForm.validate();
    UsuarioModel.save(UsuarioForm.getData());
    return Response(200);
  }

  Response function getConvites() {
    var result = {};
    result.solicitados = ConviteModel.find(status: 'solicitado|doado', solicitanteId: CurrentUser.id);
    result.solicitadosConfirmados = ConviteModel.find(status: 'confirmado', solicitanteId: CurrentUser.id);
    result.doados = ConviteModel.find(status: 'doado', doadorId: CurrentUser.id);
    result.doadosConfirmados = ConviteModel.find(status: 'confirmado', doadorId: CurrentUser.id);
    return Response(200, result);
  }
}

class AppsController {
  Response function getList() {
    var list = AppModel.find(status == 'ativo');
    return Response(200, list);
  }
}

// auth
//   login
//   logout
// user
//   register
//   settings
// invite
//   list
//   need
//   donate
//   my invites
//     requested
//     sent
