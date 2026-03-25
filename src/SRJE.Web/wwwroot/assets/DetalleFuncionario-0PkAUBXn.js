import{i as ot,_ as at,n as nt,E as R,d as n,o as d,c,a as e,b as r,k as p,t as s,q as z,f as C,U,G as st,B as _,s as E,x as w,j as S,w as it,e as F,y as N,v as j,T as lt,J as dt,g as v,A as rt,m as ct,z as ut,h as pt}from"./index-CrLvX-J6.js";import{u as mt}from"./funcionarios-WpYMRzd-.js";import{f as ft}from"./useFormato-D2DftH4l.js";import{A as vt,C as bt}from"./AlertMessage-BgA294NE.js";import{C as gt}from"./ConfirmModal-DAcEBz07.js";import{U as xt,A as H,a as Ct}from"./user-x-BlpH8jeq.js";import{D as yt}from"./dollar-sign-YQWIPhzw.js";import{C as Bt}from"./circle-check-cdeUMHv3.js";import{P as G}from"./pencil-CdxiaMkM.js";import{B as ht}from"./ban-BXma2UDo.js";import{L as q}from"./landmark-BCCaHXMf.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const kt=ot("printer",[["path",{d:"M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2",key:"143wyd"}],["path",{d:"M6 9V3a1 1 0 0 1 1-1h10a1 1 0 0 1 1 1v6",key:"1itne7"}],["rect",{x:"6",y:"14",width:"12",height:"8",rx:"1",key:"1ue0tg"}]]),$t={key:0,class:"detalle-funcionario"},zt={class:"page-header"},Et={class:"stats-bar"},wt={class:"stat-card"},St={class:"stat-content"},Ft={class:"stat-value"},Tt={class:"stat-card"},Pt={class:"stat-content"},At={class:"stat-value"},Mt={class:"sections"},It={key:1,class:"card-section"},Lt={class:"beneficiario-header"},Vt={class:"beneficiario-info"},Ot={class:"text-muted"},Dt={class:"beneficiario-cuenta"},Rt=["onClick"],Ut={key:0,class:"data-table"},_t={key:1,class:"text-muted",style:{padding:"0.5rem 0","font-size":"0.85rem"}},Nt={class:"actions"},jt={class:"modal-dialog modal-dialog-lg"},Ht={class:"modal-header"},Gt={class:"modal-body"},qt={style:{"margin-bottom":"1rem","font-size":"0.85rem",color:"var(--text-secondary)"}},Xt={class:"form-grid"},Jt={class:"field"},Yt=["value"],Kt={class:"field"},Qt=["value"],Wt={key:0,class:"field"},Zt={key:1,class:"field"},te={class:"modal-footer"},ee=["disabled"],oe={key:1,class:"loading"},ae={key:2,class:"empty-state"},ne={__name:"DetalleFuncionario",props:{rut:{type:[String,Number],required:!0}},setup(M){const T=M,i=mt(),X=pt(),b=v(""),y=v("info"),P=v(!1),B=v(!1),$=v(!1),I=v([]),L=v([]),l=v({rutBeneficiario:null,rutFormateado:"",nombreBeneficiario:"",codBanco:null,tipoCuenta:null,ctaEstado:"",ctaOtBanco:""}),V=rt(()=>{var a;return(a=i.detalle)!=null&&a.beneficiarios?i.detalle.beneficiarios.reduce((t,f)=>{const x=(f.retenciones||[]).reduce((g,o)=>g+(o.monto||0),0);return t+x},0):0});function O(a){return a===1?"Cuenta Corriente":a===2?"Ahorro / CuentaRUT":a===3?"Cuenta Vista":"-"}nt(async()=>{i.obtener(Number(T.rut));const[a,t]=await Promise.all([R.bancos(),R.tiposCuenta()]);I.value=a.data,L.value=t.data});async function J(){await i.inactivar(Number(T.rut))?(y.value="success",b.value="Funcionario inactivado exitosamente. Redirigiendo...",setTimeout(()=>X.push("/funcionarios"),1500)):(y.value="error",b.value=i.error||"Error al inactivar el funcionario.")}function Y(a){l.value={rutBeneficiario:a.rutBeneficiario,rutFormateado:a.rutFormateado||"",nombreBeneficiario:a.nombreBeneficiario||"",codBanco:a.codBanco||null,tipoCuenta:a.tipoCuenta||null,ctaEstado:a.ctaEstado?String(a.ctaEstado).replace(/[^0-9]/g,""):"",ctaOtBanco:a.ctaOtBanco?String(a.ctaOtBanco).replace(/[^0-9]/g,""):""},B.value=!0}function K(){l.value.ctaEstado="",l.value.ctaOtBanco=""}function D(a){l.value[a]=l.value[a].replace(/[^0-9]/g,"")}function Q(){var o,h,u;const a=i.detalle,t=new Date().toLocaleDateString("es-CL");let f="";if((o=a.beneficiarios)!=null&&o.length)for(const m of a.beneficiarios){const Z=m.nombreBanco||m.codBanco||"-",tt=O(m.tipoCuenta),et=m.ctaEstado||m.ctaOtBanco||"-";let A="";(h=m.retenciones)!=null&&h.length?A=`<table class="ret-table">
          <thead><tr><th>Monto</th><th>Cod. Retencion</th><th>Tipo Pago</th><th>Periodo</th><th>Estado</th></tr></thead>
          <tbody>${m.retenciones.map(k=>`<tr>
            <td>$${(k.monto||0).toLocaleString("es-CL")}</td>
            <td>${k.codRetencion||"-"}</td>
            <td>${k.tipoPago||"-"}</td>
            <td>${k.periodoProceso||"-"}</td>
            <td>${k.estado==="A"?"Activo":"Inactivo"}</td>
          </tr>`).join("")}</tbody></table>`:A='<p class="muted">Sin retenciones activas.</p>',f+=`
        <div class="benef-block">
          <div class="benef-header">
            <strong>${m.nombreBeneficiario}</strong> <span class="rut">${m.rutFormateado}</span>
          </div>
          <div class="benef-cuenta">Banco: ${Z} | Tipo: ${tt} | Cuenta: ${et}</div>
          ${A}
        </div>`}else f='<p class="muted">Sin beneficiarios asociados.</p>';const x=`<!DOCTYPE html><html><head><meta charset="utf-8">
<title>Ficha Funcionario - ${a.rutFormateado}</title>
<style>
  * { margin: 0; padding: 0; box-sizing: border-box; }
  body { font-family: Arial, Helvetica, sans-serif; font-size: 11px; color: #1e293b; padding: 20px; }
  h1 { font-size: 16px; margin-bottom: 2px; }
  .subtitle { font-size: 12px; color: #64748b; margin-bottom: 12px; }
  .fecha { font-size: 10px; color: #94a3b8; text-align: right; margin-bottom: 10px; }
  .section { margin-bottom: 14px; }
  .section h2 { font-size: 12px; background: #f1f5f9; padding: 4px 8px; margin-bottom: 6px; border-left: 3px solid #2563eb; }
  dl { display: grid; grid-template-columns: 140px 1fr; gap: 2px 10px; padding: 0 8px; }
  dt { font-weight: bold; color: #475569; }
  dd { color: #1e293b; }
  .estado { display: inline-block; padding: 1px 8px; border-radius: 10px; font-size: 10px; font-weight: bold; }
  .estado-a { background: #dcfce7; color: #166534; }
  .estado-i { background: #fee2e2; color: #991b1b; }
  .benef-block { border: 1px solid #e2e8f0; border-radius: 4px; padding: 8px; margin-bottom: 8px; page-break-inside: avoid; }
  .benef-header { font-size: 11px; margin-bottom: 3px; }
  .benef-header .rut { color: #64748b; font-size: 10px; margin-left: 6px; }
  .benef-cuenta { font-size: 10px; color: #475569; margin-bottom: 6px; }
  .ret-table { width: 100%; border-collapse: collapse; font-size: 10px; }
  .ret-table th { background: #f1f5f9; text-align: left; padding: 3px 6px; border-bottom: 1px solid #e2e8f0; }
  .ret-table td { padding: 2px 6px; border-bottom: 1px solid #f1f5f9; }
  .muted { color: #94a3b8; font-size: 10px; font-style: italic; }
  .stats { display: flex; gap: 16px; margin-bottom: 12px; }
  .stat-box { border: 1px solid #e2e8f0; border-radius: 4px; padding: 6px 12px; text-align: center; }
  .stat-box .val { font-size: 16px; font-weight: bold; color: #2563eb; }
  .stat-box .lbl { font-size: 9px; color: #64748b; display: block; }
  @media print { body { padding: 10px; } }
</style></head><body>
<div class="fecha">Impreso: ${t}</div>
<h1>${a.nombres} ${a.apellidoPaterno} ${a.apellidoMaterno}</h1>
<p class="subtitle">RUT: ${a.rutFormateado} | <span class="estado ${a.activo==="S"?"estado-a":"estado-i"}">${a.activo==="S"?"Activo":"Inactivo"}</span></p>

<div class="stats">
  <div class="stat-box"><span class="val">${((u=a.beneficiarios)==null?void 0:u.length)||0}</span><span class="lbl">Beneficiarios</span></div>
  <div class="stat-box"><span class="val">$${V.value.toLocaleString("es-CL")}</span><span class="lbl">Monto Total</span></div>
</div>

<div class="section">
  <h2>Datos del Funcionario</h2>
  <dl>
    <dt>Apellido Paterno</dt><dd>${a.apellidoPaterno||"-"}</dd>
    <dt>Apellido Materno</dt><dd>${a.apellidoMaterno||"-"}</dd>
    <dt>Nombres</dt><dd>${a.nombres||"-"}</dd>
    <dt>ID Sistema</dt><dd>${a.idSistema||"-"}</dd>
  </dl>
</div>

<div class="section">
  <h2>Beneficiarios y Retenciones</h2>
  ${f}
</div>

<script>window.onload = function() { window.print(); }<\/script>
</body></html>`,g=window.open("","_blank","width=800,height=600");g.document.write(x),g.document.close()}async function W(){var a,t;$.value=!0;try{await ut.actualizar(l.value.rutBeneficiario,{codBanco:l.value.codBanco,tipoCuenta:l.value.tipoCuenta,ctaEstado:l.value.ctaEstado||null,ctaOtBanco:l.value.ctaOtBanco||null}),y.value="success",b.value="Cuenta bancaria actualizada exitosamente.",B.value=!1,await i.obtener(Number(T.rut))}catch(f){y.value="error",b.value=((t=(a=f.response)==null?void 0:a.data)==null?void 0:t.error)||"Error al actualizar la cuenta bancaria."}finally{$.value=!1}}return(a,t)=>{var x,g;const f=ct("router-link");return n(i).detalle?(d(),c("div",$t,[e("div",zt,[e("h1",null,[r(n(xt),{size:24}),p(" "+s(n(i).detalle.nombres)+" "+s(n(i).detalle.apellidoPaterno)+" "+s(n(i).detalle.apellidoMaterno),1)]),e("p",null,s(n(i).detalle.rutFormateado),1)]),b.value?(d(),z(vt,{key:0,message:b.value,type:y.value,onClose:t[0]||(t[0]=o=>b.value="")},null,8,["message","type"])):C("",!0),e("div",Et,[e("div",wt,[r(n(U),{size:18,class:"stat-icon"}),e("div",St,[e("span",Ft,s(((x=n(i).detalle.beneficiarios)==null?void 0:x.length)||0),1),t[11]||(t[11]=e("span",{class:"stat-label"},"Total Beneficiarios",-1))])]),e("div",Tt,[r(n(yt),{size:18,class:"stat-icon"}),e("div",Pt,[e("span",At,"$"+s(V.value.toLocaleString("es-CL")),1),t[12]||(t[12]=e("span",{class:"stat-label"},"Monto Total Retenciones",-1))])])]),e("div",Mt,[e("section",null,[e("h3",null,[r(n(st),{size:16}),t[13]||(t[13]=p(" Datos del Funcionario",-1))]),e("dl",null,[t[14]||(t[14]=e("dt",null,"Estado",-1)),e("dd",null,[e("span",{class:_("estado estado-"+(n(i).detalle.activo==="S"?"a":"i"))},[n(i).detalle.activo==="S"?(d(),z(n(Bt),{key:0,size:13})):(d(),z(n(bt),{key:1,size:13})),p(" "+s(n(i).detalle.activo==="S"?"Activo":"Inactivo"),1)],2)]),t[15]||(t[15]=e("dt",null,"Apellido Paterno",-1)),e("dd",null,s(n(i).detalle.apellidoPaterno||"-"),1),t[16]||(t[16]=e("dt",null,"Apellido Materno",-1)),e("dd",null,s(n(i).detalle.apellidoMaterno||"-"),1),t[17]||(t[17]=e("dt",null,"Nombres",-1)),e("dd",null,s(n(i).detalle.nombres||"-"),1),t[18]||(t[18]=e("dt",null,"ID Sistema",-1)),e("dd",null,s(n(i).detalle.idSistema||"-"),1)])])]),(g=n(i).detalle.beneficiarios)!=null&&g.length?(d(),c("section",It,[e("h3",null,[r(n(U),{size:16}),t[19]||(t[19]=p(" Beneficiarios",-1))]),(d(!0),c(E,null,w(n(i).detalle.beneficiarios,o=>{var h;return d(),c("div",{key:o.rutBeneficiario,class:"beneficiario-card"},[e("div",Lt,[e("div",Vt,[e("strong",null,s(o.nombreBeneficiario),1),e("span",Ot,s(o.rutFormateado),1)]),e("div",Dt,[e("span",null,[r(n(q),{size:14}),p(" "+s(o.nombreBanco||o.codBanco||"-"),1)]),e("span",null,s(O(o.tipoCuenta)),1),e("span",null,s(n(ft)(o.ctaEstado||o.ctaOtBanco)),1)]),e("button",{class:"btn-sm",onClick:u=>Y(o)},[r(n(G),{size:14}),t[20]||(t[20]=p(" Editar Cuenta ",-1))],8,Rt)]),(h=o.retenciones)!=null&&h.length?(d(),c("table",Ut,[t[21]||(t[21]=e("thead",null,[e("tr",null,[e("th",null,"Monto"),e("th",null,"Codigo Retencion"),e("th",null,"Tipo Pago"),e("th",null,"Periodo"),e("th",null,"Estado")])],-1)),e("tbody",null,[(d(!0),c(E,null,w(o.retenciones,u=>{var m;return d(),c("tr",{key:u.id},[e("td",null,"$"+s((m=u.monto)==null?void 0:m.toLocaleString("es-CL")),1),e("td",null,s(u.codRetencion),1),e("td",null,s(u.tipoPago),1),e("td",null,s(u.periodoProceso),1),e("td",null,[e("span",{class:_("estado estado-"+(u.estado||"a").toLowerCase())},s(u.estado==="A"?"Activo":u.estado==="I"?"Inactivo":u.estado||"-"),3)])])}),128))])])):(d(),c("p",_t,"Sin retenciones activas."))])}),128))])):C("",!0),e("div",Nt,[r(f,{to:`/funcionarios/${M.rut}/editar`,class:"btn btn-primary"},{default:S(()=>[r(n(G),{size:16}),t[22]||(t[22]=p(" Editar Funcionario ",-1))]),_:1},8,["to"]),e("button",{class:"btn btn-secondary",onClick:Q},[r(n(kt),{size:16}),t[23]||(t[23]=p(" Imprimir Ficha ",-1))]),n(i).detalle.activo==="S"?(d(),c("button",{key:0,class:"btn btn-danger",onClick:t[1]||(t[1]=o=>P.value=!0)},[r(n(ht),{size:16}),t[24]||(t[24]=p(" Inactivar ",-1))])):C("",!0),r(f,{to:"/funcionarios",class:"btn btn-secondary"},{default:S(()=>[r(n(H),{size:16}),t[25]||(t[25]=p(" Volver ",-1))]),_:1})]),r(gt,{modelValue:P.value,"onUpdate:modelValue":t[2]||(t[2]=o=>P.value=o),title:"Inactivar Funcionario",message:`¿Esta seguro de inactivar a ${n(i).detalle.nombres} ${n(i).detalle.apellidoPaterno}? Esta accion cambiara su estado a Inactivo.`,confirmText:"Si, inactivar",variant:"danger",onConfirm:J},null,8,["modelValue","message"]),(d(),z(dt,{to:"body"},[r(lt,{name:"modal-fade"},{default:S(()=>[B.value?(d(),c("div",{key:0,class:"modal-overlay",onClick:t[10]||(t[10]=it(o=>B.value=!1,["self"]))},[e("div",jt,[e("div",Ht,[r(n(q),{size:20,class:"modal-icon primary"}),t[26]||(t[26]=e("h3",null,"Editar Cuenta Bancaria",-1))]),e("div",Gt,[e("p",qt,[t[27]||(t[27]=p(" Beneficiario: ",-1)),e("strong",null,s(l.value.nombreBeneficiario),1),p(" ("+s(l.value.rutFormateado)+") ",1)]),e("div",Xt,[e("div",Jt,[t[29]||(t[29]=e("label",null,"Banco",-1)),F(e("select",{"onUpdate:modelValue":t[3]||(t[3]=o=>l.value.codBanco=o),onChange:K},[t[28]||(t[28]=e("option",{value:null},"-- Seleccionar banco --",-1)),(d(!0),c(E,null,w(I.value,o=>(d(),c("option",{key:o.codBanco,value:o.codBanco},s(o.codBanco)+" - "+s(o.nombreBanco),9,Yt))),128))],544),[[N,l.value.codBanco]])]),e("div",Kt,[t[31]||(t[31]=e("label",null,"Tipo Cuenta",-1)),F(e("select",{"onUpdate:modelValue":t[4]||(t[4]=o=>l.value.tipoCuenta=o)},[t[30]||(t[30]=e("option",{value:null},"-- Seleccionar tipo --",-1)),(d(!0),c(E,null,w(L.value,o=>(d(),c("option",{key:o.codTipoCuenta,value:o.codTipoCuenta},s(o.codTipoCuenta)+" - "+s(o.descripcion),9,Qt))),128))],512),[[N,l.value.tipoCuenta]])]),l.value.codBanco===12?(d(),c("div",Wt,[t[32]||(t[32]=e("label",null,"Cuenta BancoEstado",-1)),F(e("input",{"onUpdate:modelValue":t[5]||(t[5]=o=>l.value.ctaEstado=o),maxlength:"11",placeholder:"Ej: 41762633599",onInput:t[6]||(t[6]=o=>D("ctaEstado"))},null,544),[[j,l.value.ctaEstado]])])):C("",!0),l.value.codBanco&&l.value.codBanco!==12?(d(),c("div",Zt,[t[33]||(t[33]=e("label",null,"Cuenta Otro Banco",-1)),F(e("input",{"onUpdate:modelValue":t[7]||(t[7]=o=>l.value.ctaOtBanco=o),maxlength:"15",onInput:t[8]||(t[8]=o=>D("ctaOtBanco"))},null,544),[[j,l.value.ctaOtBanco]])])):C("",!0)])]),e("div",te,[e("button",{class:"btn btn-secondary",onClick:t[9]||(t[9]=o=>B.value=!1)},"Cancelar"),e("button",{class:"btn btn-primary",onClick:W,disabled:$.value},s($.value?"Guardando...":"Guardar"),9,ee)])])])):C("",!0)]),_:1})]))])):n(i).loading?(d(),c("div",oe,"Cargando datos del funcionario...")):(d(),c("div",ae,[r(n(Ct),{size:48,class:"empty-icon"}),t[35]||(t[35]=e("p",null,"Funcionario no encontrado",-1)),r(f,{to:"/funcionarios",class:"btn btn-secondary",style:{"margin-top":"0.5rem"}},{default:S(()=>[r(n(H),{size:16}),t[34]||(t[34]=p(" Volver a la lista ",-1))]),_:1})]))}}},be=at(ne,[["__scopeId","data-v-ef171495"]]);export{be as default};
