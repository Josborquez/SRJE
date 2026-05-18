import{i as j,n as H,p as O,d as o,o as d,c,a as t,b as r,k as m,t as i,x as k,f as C,I as X,E as q,y as B,z,S as Y,j as x,g as F,D as G,m as J,h as K}from"./index-BNwbpPN2.js";import{f as D}from"./useFormato-D2DftH4l.js";import{A as Q,C as W}from"./AlertMessage-COB80QmQ.js";import{C as Z}from"./ConfirmModal-BfT1QtCV.js";import{U as _,P as tt,A as M,a as et}from"./user-x-C_GDBNVH.js";import{C as ot}from"./circle-check-DoIQkmxG.js";import{L as V}from"./landmark-BRqQ_Mna.js";import{P as nt}from"./pencil-BKcpOlfA.js";import{B as at}from"./ban-j4bHoo7u.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const lt=j("briefcase",[["path",{d:"M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16",key:"jecpp"}],["rect",{width:"20",height:"14",x:"2",y:"6",rx:"2",key:"i6l2r4"}]]),it={key:0,class:"detalle-beneficiario"},st={class:"page-header"},dt={class:"sections"},rt={key:0},ut={style:{"margin-left":"0.5rem",color:"var(--text-secondary)"}},ct={key:1,style:{color:"var(--text-muted)","font-size":"0.85rem"}},mt={key:0},pt={class:"data-table"},bt={key:1,class:"card-section"},ft={class:"data-table"},gt={class:"actions"},vt={key:1,class:"loading"},ht={key:2,class:"empty-state"},Tt={__name:"DetalleBeneficiario",props:{rut:{type:[String,Number],required:!0}},setup(w){const T=w,n=H(),I=K(),f=F(""),y=F("info"),$=F(!1),A=G(()=>{var e;const a=(e=n.detalle)==null?void 0:e.tipoCuenta;return a===1?"Cuenta Corriente":a===2?"Ahorro / CuentaRUT":a===3?"Cuenta Vista":"-"});O(()=>{n.detalle=null,n.obtener(Number(T.rut))});function s(a){const e=document.createElement("div");return e.textContent=a??"",e.innerHTML}function N(){var P,L,E;const a=n.detalle,e=new Date().toLocaleDateString("es-CL"),p=a.sexo==="M"?"Masculino":a.sexo==="F"?"Femenino":"-",g=A.value,v=a.ctaEstado||a.ctaOtBanco||"-",h=a.nombreBanco||a.codBanco||"-";let l="";(P=a.funcionarios)!=null&&P.length?l=a.funcionarios.map(u=>`<div style="margin-bottom: 3px;"><strong>${s(u.rutFormateado)}</strong> <span style="color: #64748b; margin-left: 6px;">${s(u.nombreCompleto)}</span></div>`).join(""):l='<p class="muted">Sin funcionarios asociados.</p>';let b="";(L=a.retenciones)!=null&&L.length?b=`<table class="ret-table">
      <thead><tr><th>Funcionario</th><th>Monto</th><th>Codigo</th><th>Tipo Pago</th><th>Periodo</th></tr></thead>
      <tbody>${a.retenciones.map(u=>`<tr>
        <td>${s(u.nombreFuncionario||u.rutTitularFormateado||"-")}</td>
        <td>$${(u.monto||0).toLocaleString("es-CL")}</td>
        <td>${s(u.codRetencion||"-")}</td>
        <td>${s(u.tipoPago||"-")}</td>
        <td>${s(u.periodoProceso||"-")}</td>
      </tr>`).join("")}</tbody></table>`:b='<p class="muted">Sin retenciones activas.</p>';const U=`<!DOCTYPE html><html><head><meta charset="utf-8">
<title>Ficha Beneficiario - ${a.rutFormateado}</title>
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
  .ret-table { width: 100%; border-collapse: collapse; font-size: 10px; }
  .ret-table th { background: #f1f5f9; text-align: left; padding: 3px 6px; border-bottom: 1px solid #e2e8f0; }
  .ret-table td { padding: 2px 6px; border-bottom: 1px solid #f1f5f9; }
  .muted { color: #94a3b8; font-size: 10px; font-style: italic; }
  @media print { body { padding: 10px; } }
</style></head><body>
<div class="fecha">Impreso: ${e}</div>
<h1>${s(a.nombreBeneficiario)}</h1>
<p class="subtitle">RUT: ${s(a.rutFormateado)} | <span class="estado ${a.estado==="A"?"estado-a":"estado-i"}">${a.estado==="A"?"Activo":"Inactivo"}</span></p>

<div class="section">
  <h2>Datos Personales</h2>
  <dl>
    <dt>Sexo</dt><dd>${s(p)}</dd>
    <dt>Estado Civil</dt><dd>${s(a.estadoCivil||"-")}</dd>
    <dt>Domicilio</dt><dd>${s(a.domicilio||"-")}</dd>
    <dt>Comuna</dt><dd>${s(a.comuna||"-")}</dd>
    <dt>Telefono</dt><dd>${s(a.telefono||"-")}</dd>
  </dl>
</div>

<div class="section">
  <h2>Funcionario(s) Titular(es)</h2>
  ${l}
</div>

<div class="section">
  <h2>Cuenta Bancaria Principal</h2>
  <dl>
    <dt>Banco</dt><dd>${s(h)}</dd>
    <dt>Tipo Cuenta</dt><dd>${s(g)}</dd>
    <dt>Cuenta</dt><dd>${s(v)}</dd>
    <dt>Sucursal</dt><dd>${s(a.sucursal||"-")}</dd>
  </dl>
</div>

${(E=a.cuentas)!=null&&E.length?`<div class="section">
  <h2>Cuentas Almacenadas (${a.cuentas.length})</h2>
  <table class="ret-table">
    <thead><tr><th>Banco</th><th>Tipo Cuenta</th><th>N° Cuenta</th><th>Alias</th></tr></thead>
    <tbody>${a.cuentas.map(u=>`<tr>
      <td>${s(u.nombreBanco||String(u.codBanco))}</td>
      <td>${s(u.tipoCuentaDescripcion||String(u.tipoCuenta))}</td>
      <td>${s(u.numeroCuenta||"-")}</td>
      <td>${s(u.alias||"-")}</td>
    </tr>`).join("")}</tbody>
  </table>
</div>`:""}

<div class="section">
  <h2>Retenciones Activas</h2>
  ${b}
</div>

<script>window.onload = function() { window.print(); window.close(); }<\/script>
</body></html>`,S=window.open("","_blank","width=800,height=600");S.document.write(U),S.document.close()}async function R(){await n.inactivar(Number(T.rut))?(y.value="success",f.value="Beneficiario inactivado exitosamente. Redirigiendo...",setTimeout(()=>I.push("/beneficiarios"),1500)):(y.value="error",f.value=n.error||"Error al inactivar el beneficiario.")}return(a,e)=>{var g,v,h;const p=J("router-link");return o(n).detalle?(d(),c("div",it,[t("div",st,[t("h1",null,[r(o(_),{size:24}),m(" "+i(o(n).detalle.nombreBeneficiario),1)]),t("p",null,i(o(n).detalle.rutFormateado),1)]),f.value?(d(),k(Q,{key:0,message:f.value,type:y.value,onClose:e[0]||(e[0]=l=>f.value="")},null,8,["message","type"])):C("",!0),t("div",dt,[t("section",null,[t("h3",null,[r(o(X),{size:16}),e[3]||(e[3]=m(" Datos Personales",-1))]),t("dl",null,[e[4]||(e[4]=t("dt",null,"Estado",-1)),t("dd",null,[t("span",{class:q("estado estado-"+o(n).detalle.estado.toLowerCase())},[o(n).detalle.estado==="A"?(d(),k(o(ot),{key:0,size:13})):(d(),k(o(W),{key:1,size:13})),m(" "+i(o(n).detalle.estado==="A"?"Activo":"Inactivo"),1)],2)]),e[5]||(e[5]=t("dt",null,"Sexo",-1)),t("dd",null,i(o(n).detalle.sexo==="M"?"Masculino":o(n).detalle.sexo==="F"?"Femenino":"-"),1),e[6]||(e[6]=t("dt",null,"Estado Civil",-1)),t("dd",null,i(o(n).detalle.estadoCivil||"-"),1),e[7]||(e[7]=t("dt",null,"Domicilio",-1)),t("dd",null,i(o(n).detalle.domicilio||"-"),1),e[8]||(e[8]=t("dt",null,"Comuna",-1)),t("dd",null,i(o(n).detalle.comuna||"-"),1),e[9]||(e[9]=t("dt",null,"Telefono",-1)),t("dd",null,i(o(n).detalle.telefono||"-"),1)])]),t("section",null,[t("h3",null,[r(o(lt),{size:16}),e[10]||(e[10]=m(" Funcionario(s) Titular(es)",-1))]),(g=o(n).detalle.funcionarios)!=null&&g.length?(d(),c("div",rt,[(d(!0),c(B,null,z(o(n).detalle.funcionarios,l=>(d(),c("div",{key:l.rutFuncionario,style:{"margin-bottom":"0.4rem"}},[r(p,{to:`/funcionarios/${l.rutFuncionario}`,style:{"font-weight":"500"}},{default:x(()=>[m(i(l.rutFormateado),1)]),_:2},1032,["to"]),t("span",ut,i(l.nombreCompleto||""),1)]))),128))])):(d(),c("p",ct,"Sin funcionarios asociados (sin retenciones activas)."))]),t("section",null,[t("h3",null,[r(o(V),{size:16}),e[11]||(e[11]=m(" Cuenta Bancaria Principal",-1))]),t("dl",null,[e[12]||(e[12]=t("dt",null,"Banco",-1)),t("dd",null,i(o(n).detalle.nombreBanco||o(n).detalle.codBanco||"-"),1),e[13]||(e[13]=t("dt",null,"Tipo Cuenta",-1)),t("dd",null,i(A.value),1),e[14]||(e[14]=t("dt",null,"Cuenta",-1)),t("dd",null,i(o(D)(o(n).detalle.ctaEstado||o(n).detalle.ctaOtBanco)),1),e[15]||(e[15]=t("dt",null,"Sucursal",-1)),t("dd",null,i(o(n).detalle.sucursal||"-"),1)])]),(v=o(n).detalle.cuentas)!=null&&v.length?(d(),c("section",mt,[t("h3",null,[r(o(V),{size:16}),m(" Cuentas Almacenadas ("+i(o(n).detalle.cuentas.length)+")",1)]),t("table",pt,[e[16]||(e[16]=t("thead",null,[t("tr",null,[t("th",null,"Banco"),t("th",null,"Tipo Cuenta"),t("th",null,"N° Cuenta"),t("th",null,"Alias")])],-1)),t("tbody",null,[(d(!0),c(B,null,z(o(n).detalle.cuentas,l=>(d(),c("tr",{key:l.id},[t("td",null,i(l.nombreBanco||l.codBanco),1),t("td",null,i(l.tipoCuentaDescripcion||l.tipoCuenta),1),t("td",null,i(o(D)(l.numeroCuenta)),1),t("td",null,i(l.alias||"-"),1)]))),128))])])])):C("",!0)]),(h=o(n).detalle.retenciones)!=null&&h.length?(d(),c("section",bt,[t("h3",null,[r(o(Y),{size:16}),e[17]||(e[17]=m(" Retenciones Activas",-1))]),t("table",ft,[e[18]||(e[18]=t("thead",null,[t("tr",null,[t("th",null,"Funcionario"),t("th",null,"Monto"),t("th",null,"Codigo"),t("th",null,"Tipo Pago"),t("th",null,"Periodo")])],-1)),t("tbody",null,[(d(!0),c(B,null,z(o(n).detalle.retenciones,l=>{var b;return d(),c("tr",{key:l.id},[t("td",null,i(l.nombreFuncionario||l.rutTitularFormateado),1),t("td",null,"$"+i((b=l.monto)==null?void 0:b.toLocaleString("es-CL")),1),t("td",null,i(l.codRetencion),1),t("td",null,i(l.tipoPago),1),t("td",null,i(l.periodoProceso),1)])}),128))])])])):C("",!0),t("div",gt,[r(p,{to:`/beneficiarios/${w.rut}/editar`,class:"btn btn-primary"},{default:x(()=>[r(o(nt),{size:16}),e[19]||(e[19]=m(" Editar ",-1))]),_:1},8,["to"]),t("button",{class:"btn btn-secondary",onClick:N},[r(o(tt),{size:16}),e[20]||(e[20]=m(" Imprimir Ficha ",-1))]),o(n).detalle.estado==="A"?(d(),c("button",{key:0,class:"btn btn-danger",onClick:e[1]||(e[1]=l=>$.value=!0)},[r(o(at),{size:16}),e[21]||(e[21]=m(" Inactivar ",-1))])):C("",!0),r(p,{to:"/beneficiarios",class:"btn btn-secondary"},{default:x(()=>[r(o(M),{size:16}),e[22]||(e[22]=m(" Volver ",-1))]),_:1})]),r(Z,{modelValue:$.value,"onUpdate:modelValue":e[2]||(e[2]=l=>$.value=l),title:"Inactivar Beneficiario",message:`¿Esta seguro de inactivar a ${o(n).detalle.nombreBeneficiario}? Esta accion cambiara su estado a Inactivo.`,confirmText:"Si, inactivar",variant:"danger",onConfirm:R},null,8,["modelValue","message"])])):o(n).loading?(d(),c("div",vt,"Cargando datos del beneficiario...")):(d(),c("div",ht,[r(o(et),{size:48,class:"empty-icon"}),e[24]||(e[24]=t("p",null,"Beneficiario no encontrado",-1)),r(p,{to:"/beneficiarios",class:"btn btn-secondary",style:{"margin-top":"0.5rem"}},{default:x(()=>[r(o(M),{size:16}),e[23]||(e[23]=m(" Volver a la lista ",-1))]),_:1})]))}}};export{Tt as default};
