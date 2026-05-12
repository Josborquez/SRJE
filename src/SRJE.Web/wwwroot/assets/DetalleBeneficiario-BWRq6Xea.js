import{i as I,n as R,d as o,o as d,c as u,a as e,b as s,k as r,t as i,q as h,f as k,G as N,B as U,s as S,x as L,S as j,j as v,g as $,A as H,m as O,h as q}from"./index-CUjzb1F-.js";import{u as X}from"./beneficiarios-DCjAIJhj.js";import{f as G}from"./useFormato-D2DftH4l.js";import{A as Y,C as J}from"./AlertMessage-DPddWEw6.js";import{C as K}from"./ConfirmModal-WpU0k6hu.js";import{U as Q,P as W,A as P,a as Z}from"./user-x-CqCYDwKa.js";import{C as _}from"./circle-check-DPBQgQNh.js";import{L as tt}from"./landmark-UkvRVBnm.js";import{P as et}from"./pencil-C5D7MTin.js";import{B as ot}from"./ban-kX7TR4c3.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const nt=I("briefcase",[["path",{d:"M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16",key:"jecpp"}],["rect",{width:"20",height:"14",x:"2",y:"6",rx:"2",key:"i6l2r4"}]]),at={key:0,class:"detalle-beneficiario"},it={class:"page-header"},lt={class:"sections"},st={key:0},dt={style:{"margin-left":"0.5rem",color:"var(--text-secondary)"}},rt={key:1,style:{color:"var(--text-muted)","font-size":"0.85rem"}},ut={key:1,class:"card-section"},ct={class:"data-table"},mt={class:"actions"},pt={key:1,class:"loading"},ft={key:2,class:"empty-state"},zt={__name:"DetalleBeneficiario",props:{rut:{type:[String,Number],required:!0}},setup(B){const z=B,n=X(),E=q(),f=$(""),x=$("info"),y=$(!1),F=H(()=>{var t;const a=(t=n.detalle)==null?void 0:t.tipoCuenta;return a===1?"Cuenta Corriente":a===2?"Ahorro / CuentaRUT":a===3?"Cuenta Vista":"-"});R(()=>n.obtener(Number(z.rut)));function M(){var T,A;const a=n.detalle,t=new Date().toLocaleDateString("es-CL"),m=a.sexo==="M"?"Masculino":a.sexo==="F"?"Femenino":"-",b=F.value,g=a.ctaEstado||a.ctaOtBanco||"-",l=a.nombreBanco||a.codBanco||"-";let p="";(T=a.funcionarios)!=null&&T.length?p=a.funcionarios.map(c=>`<div style="margin-bottom: 3px;"><strong>${c.rutFormateado}</strong> <span style="color: #64748b; margin-left: 6px;">${c.nombreCompleto||""}</span></div>`).join(""):p='<p class="muted">Sin funcionarios asociados.</p>';let C="";(A=a.retenciones)!=null&&A.length?C=`<table class="ret-table">
      <thead><tr><th>Funcionario</th><th>Monto</th><th>Codigo</th><th>Tipo Pago</th><th>Periodo</th></tr></thead>
      <tbody>${a.retenciones.map(c=>`<tr>
        <td>${c.nombreFuncionario||c.rutTitularFormateado||"-"}</td>
        <td>$${(c.monto||0).toLocaleString("es-CL")}</td>
        <td>${c.codRetencion||"-"}</td>
        <td>${c.tipoPago||"-"}</td>
        <td>${c.periodoProceso||"-"}</td>
      </tr>`).join("")}</tbody></table>`:C='<p class="muted">Sin retenciones activas.</p>';const D=`<!DOCTYPE html><html><head><meta charset="utf-8">
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
<div class="fecha">Impreso: ${t}</div>
<h1>${a.nombreBeneficiario}</h1>
<p class="subtitle">RUT: ${a.rutFormateado} | <span class="estado ${a.estado==="A"?"estado-a":"estado-i"}">${a.estado==="A"?"Activo":"Inactivo"}</span></p>

<div class="section">
  <h2>Datos Personales</h2>
  <dl>
    <dt>Sexo</dt><dd>${m}</dd>
    <dt>Estado Civil</dt><dd>${a.estadoCivil||"-"}</dd>
    <dt>Domicilio</dt><dd>${a.domicilio||"-"}</dd>
    <dt>Comuna</dt><dd>${a.comuna||"-"}</dd>
    <dt>Telefono</dt><dd>${a.telefono||"-"}</dd>
  </dl>
</div>

<div class="section">
  <h2>Funcionario(s) Titular(es)</h2>
  ${p}
</div>

<div class="section">
  <h2>Cuenta Bancaria</h2>
  <dl>
    <dt>Banco</dt><dd>${l}</dd>
    <dt>Tipo Cuenta</dt><dd>${b}</dd>
    <dt>Cuenta</dt><dd>${g}</dd>
    <dt>Sucursal</dt><dd>${a.sucursal||"-"}</dd>
  </dl>
</div>

<div class="section">
  <h2>Retenciones Activas</h2>
  ${C}
</div>

<script>window.onload = function() { window.print(); }<\/script>
</body></html>`,w=window.open("","_blank","width=800,height=600");w.document.write(D),w.document.close()}async function V(){await n.inactivar(Number(z.rut))?(x.value="success",f.value="Beneficiario inactivado exitosamente. Redirigiendo...",setTimeout(()=>E.push("/beneficiarios"),1500)):(x.value="error",f.value=n.error||"Error al inactivar el beneficiario.")}return(a,t)=>{var b,g;const m=O("router-link");return o(n).detalle?(d(),u("div",at,[e("div",it,[e("h1",null,[s(o(Q),{size:24}),r(" "+i(o(n).detalle.nombreBeneficiario),1)]),e("p",null,i(o(n).detalle.rutFormateado),1)]),f.value?(d(),h(Y,{key:0,message:f.value,type:x.value,onClose:t[0]||(t[0]=l=>f.value="")},null,8,["message","type"])):k("",!0),e("div",lt,[e("section",null,[e("h3",null,[s(o(N),{size:16}),t[3]||(t[3]=r(" Datos Personales",-1))]),e("dl",null,[t[4]||(t[4]=e("dt",null,"Estado",-1)),e("dd",null,[e("span",{class:U("estado estado-"+o(n).detalle.estado.toLowerCase())},[o(n).detalle.estado==="A"?(d(),h(o(_),{key:0,size:13})):(d(),h(o(J),{key:1,size:13})),r(" "+i(o(n).detalle.estado==="A"?"Activo":"Inactivo"),1)],2)]),t[5]||(t[5]=e("dt",null,"Sexo",-1)),e("dd",null,i(o(n).detalle.sexo==="M"?"Masculino":o(n).detalle.sexo==="F"?"Femenino":"-"),1),t[6]||(t[6]=e("dt",null,"Estado Civil",-1)),e("dd",null,i(o(n).detalle.estadoCivil||"-"),1),t[7]||(t[7]=e("dt",null,"Domicilio",-1)),e("dd",null,i(o(n).detalle.domicilio||"-"),1),t[8]||(t[8]=e("dt",null,"Comuna",-1)),e("dd",null,i(o(n).detalle.comuna||"-"),1),t[9]||(t[9]=e("dt",null,"Telefono",-1)),e("dd",null,i(o(n).detalle.telefono||"-"),1)])]),e("section",null,[e("h3",null,[s(o(nt),{size:16}),t[10]||(t[10]=r(" Funcionario(s) Titular(es)",-1))]),(b=o(n).detalle.funcionarios)!=null&&b.length?(d(),u("div",st,[(d(!0),u(S,null,L(o(n).detalle.funcionarios,l=>(d(),u("div",{key:l.rutFuncionario,style:{"margin-bottom":"0.4rem"}},[s(m,{to:`/funcionarios/${l.rutFuncionario}`,style:{"font-weight":"500"}},{default:v(()=>[r(i(l.rutFormateado),1)]),_:2},1032,["to"]),e("span",dt,i(l.nombreCompleto||""),1)]))),128))])):(d(),u("p",rt,"Sin funcionarios asociados (sin retenciones activas)."))]),e("section",null,[e("h3",null,[s(o(tt),{size:16}),t[11]||(t[11]=r(" Cuenta Bancaria",-1))]),e("dl",null,[t[12]||(t[12]=e("dt",null,"Banco",-1)),e("dd",null,i(o(n).detalle.nombreBanco||o(n).detalle.codBanco||"-"),1),t[13]||(t[13]=e("dt",null,"Tipo Cuenta",-1)),e("dd",null,i(F.value),1),t[14]||(t[14]=e("dt",null,"Cuenta",-1)),e("dd",null,i(o(G)(o(n).detalle.ctaEstado||o(n).detalle.ctaOtBanco)),1),t[15]||(t[15]=e("dt",null,"Sucursal",-1)),e("dd",null,i(o(n).detalle.sucursal||"-"),1)])])]),(g=o(n).detalle.retenciones)!=null&&g.length?(d(),u("section",ut,[e("h3",null,[s(o(j),{size:16}),t[16]||(t[16]=r(" Retenciones Activas",-1))]),e("table",ct,[t[17]||(t[17]=e("thead",null,[e("tr",null,[e("th",null,"Funcionario"),e("th",null,"Monto"),e("th",null,"Codigo"),e("th",null,"Tipo Pago"),e("th",null,"Periodo")])],-1)),e("tbody",null,[(d(!0),u(S,null,L(o(n).detalle.retenciones,l=>{var p;return d(),u("tr",{key:l.id},[e("td",null,i(l.nombreFuncionario||l.rutTitularFormateado),1),e("td",null,"$"+i((p=l.monto)==null?void 0:p.toLocaleString("es-CL")),1),e("td",null,i(l.codRetencion),1),e("td",null,i(l.tipoPago),1),e("td",null,i(l.periodoProceso),1)])}),128))])])])):k("",!0),e("div",mt,[s(m,{to:`/beneficiarios/${B.rut}/editar`,class:"btn btn-primary"},{default:v(()=>[s(o(et),{size:16}),t[18]||(t[18]=r(" Editar ",-1))]),_:1},8,["to"]),e("button",{class:"btn btn-secondary",onClick:M},[s(o(W),{size:16}),t[19]||(t[19]=r(" Imprimir Ficha ",-1))]),o(n).detalle.estado==="A"?(d(),u("button",{key:0,class:"btn btn-danger",onClick:t[1]||(t[1]=l=>y.value=!0)},[s(o(ot),{size:16}),t[20]||(t[20]=r(" Inactivar ",-1))])):k("",!0),s(m,{to:"/beneficiarios",class:"btn btn-secondary"},{default:v(()=>[s(o(P),{size:16}),t[21]||(t[21]=r(" Volver ",-1))]),_:1})]),s(K,{modelValue:y.value,"onUpdate:modelValue":t[2]||(t[2]=l=>y.value=l),title:"Inactivar Beneficiario",message:`¿Esta seguro de inactivar a ${o(n).detalle.nombreBeneficiario}? Esta accion cambiara su estado a Inactivo.`,confirmText:"Si, inactivar",variant:"danger",onConfirm:V},null,8,["modelValue","message"])])):o(n).loading?(d(),u("div",pt,"Cargando datos del beneficiario...")):(d(),u("div",ft,[s(o(Z),{size:48,class:"empty-icon"}),t[23]||(t[23]=e("p",null,"Beneficiario no encontrado",-1)),s(m,{to:"/beneficiarios",class:"btn btn-secondary",style:{"margin-top":"0.5rem"}},{default:v(()=>[s(o(P),{size:16}),t[22]||(t[22]=r(" Volver a la lista ",-1))]),_:1})]))}}};export{zt as default};
