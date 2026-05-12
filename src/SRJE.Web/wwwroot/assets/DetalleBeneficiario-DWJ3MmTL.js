import{i as R,n as N,d as o,o as r,c,a as e,b as d,k as u,t as i,s as k,f as $,H as U,D as j,x as A,y as P,S as H,j as x,g as B,B as O,m as X,h as q}from"./index-CrhNKCOX.js";import{u as Y}from"./beneficiarios-DAoVnH6M.js";import{f as G}from"./useFormato-D2DftH4l.js";import{A as J,C as K}from"./AlertMessage-ByFiHHFk.js";import{C as Q}from"./ConfirmModal-XMjhnf5A.js";import{U as W,P as Z,A as E,a as _}from"./user-x-B-Mlkixu.js";import{C as tt}from"./circle-check-Dc-XE_nz.js";import{L as et}from"./landmark-DUQIaa63.js";import{P as ot}from"./pencil-CIqfCGPp.js";import{B as nt}from"./ban-DSfnZEfq.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const at=R("briefcase",[["path",{d:"M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16",key:"jecpp"}],["rect",{width:"20",height:"14",x:"2",y:"6",rx:"2",key:"i6l2r4"}]]),it={key:0,class:"detalle-beneficiario"},lt={class:"page-header"},st={class:"sections"},dt={key:0},rt={style:{"margin-left":"0.5rem",color:"var(--text-secondary)"}},ut={key:1,style:{color:"var(--text-muted)","font-size":"0.85rem"}},ct={key:1,class:"card-section"},mt={class:"data-table"},pt={class:"actions"},ft={key:1,class:"loading"},bt={key:2,class:"empty-state"},Ft={__name:"DetalleBeneficiario",props:{rut:{type:[String,Number],required:!0}},setup(z){const F=z,n=Y(),M=q(),b=B(""),y=B("info"),C=B(!1),w=O(()=>{var t;const a=(t=n.detalle)==null?void 0:t.tipoCuenta;return a===1?"Cuenta Corriente":a===2?"Ahorro / CuentaRUT":a===3?"Cuenta Vista":"-"});N(()=>n.obtener(Number(F.rut)));function s(a){const t=document.createElement("div");return t.textContent=a??"",t.innerHTML}function V(){var L,S;const a=n.detalle,t=new Date().toLocaleDateString("es-CL"),p=a.sexo==="M"?"Masculino":a.sexo==="F"?"Femenino":"-",g=w.value,v=a.ctaEstado||a.ctaOtBanco||"-",l=a.nombreBanco||a.codBanco||"-";let f="";(L=a.funcionarios)!=null&&L.length?f=a.funcionarios.map(m=>`<div style="margin-bottom: 3px;"><strong>${s(m.rutFormateado)}</strong> <span style="color: #64748b; margin-left: 6px;">${s(m.nombreCompleto)}</span></div>`).join(""):f='<p class="muted">Sin funcionarios asociados.</p>';let h="";(S=a.retenciones)!=null&&S.length?h=`<table class="ret-table">
      <thead><tr><th>Funcionario</th><th>Monto</th><th>Codigo</th><th>Tipo Pago</th><th>Periodo</th></tr></thead>
      <tbody>${a.retenciones.map(m=>`<tr>
        <td>${s(m.nombreFuncionario||m.rutTitularFormateado||"-")}</td>
        <td>$${(m.monto||0).toLocaleString("es-CL")}</td>
        <td>${s(m.codRetencion||"-")}</td>
        <td>${s(m.tipoPago||"-")}</td>
        <td>${s(m.periodoProceso||"-")}</td>
      </tr>`).join("")}</tbody></table>`:h='<p class="muted">Sin retenciones activas.</p>';const I=`<!DOCTYPE html><html><head><meta charset="utf-8">
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
  ${f}
</div>

<div class="section">
  <h2>Cuenta Bancaria</h2>
  <dl>
    <dt>Banco</dt><dd>${s(l)}</dd>
    <dt>Tipo Cuenta</dt><dd>${s(g)}</dd>
    <dt>Cuenta</dt><dd>${s(v)}</dd>
    <dt>Sucursal</dt><dd>${s(a.sucursal||"-")}</dd>
  </dl>
</div>

<div class="section">
  <h2>Retenciones Activas</h2>
  ${h}
</div>

<script>window.onload = function() { window.print(); window.close(); }<\/script>
</body></html>`,T=window.open("","_blank","width=800,height=600");T.document.write(I),T.document.close()}async function D(){await n.inactivar(Number(F.rut))?(y.value="success",b.value="Beneficiario inactivado exitosamente. Redirigiendo...",setTimeout(()=>M.push("/beneficiarios"),1500)):(y.value="error",b.value=n.error||"Error al inactivar el beneficiario.")}return(a,t)=>{var g,v;const p=X("router-link");return o(n).detalle?(r(),c("div",it,[e("div",lt,[e("h1",null,[d(o(W),{size:24}),u(" "+i(o(n).detalle.nombreBeneficiario),1)]),e("p",null,i(o(n).detalle.rutFormateado),1)]),b.value?(r(),k(J,{key:0,message:b.value,type:y.value,onClose:t[0]||(t[0]=l=>b.value="")},null,8,["message","type"])):$("",!0),e("div",st,[e("section",null,[e("h3",null,[d(o(U),{size:16}),t[3]||(t[3]=u(" Datos Personales",-1))]),e("dl",null,[t[4]||(t[4]=e("dt",null,"Estado",-1)),e("dd",null,[e("span",{class:j("estado estado-"+o(n).detalle.estado.toLowerCase())},[o(n).detalle.estado==="A"?(r(),k(o(tt),{key:0,size:13})):(r(),k(o(K),{key:1,size:13})),u(" "+i(o(n).detalle.estado==="A"?"Activo":"Inactivo"),1)],2)]),t[5]||(t[5]=e("dt",null,"Sexo",-1)),e("dd",null,i(o(n).detalle.sexo==="M"?"Masculino":o(n).detalle.sexo==="F"?"Femenino":"-"),1),t[6]||(t[6]=e("dt",null,"Estado Civil",-1)),e("dd",null,i(o(n).detalle.estadoCivil||"-"),1),t[7]||(t[7]=e("dt",null,"Domicilio",-1)),e("dd",null,i(o(n).detalle.domicilio||"-"),1),t[8]||(t[8]=e("dt",null,"Comuna",-1)),e("dd",null,i(o(n).detalle.comuna||"-"),1),t[9]||(t[9]=e("dt",null,"Telefono",-1)),e("dd",null,i(o(n).detalle.telefono||"-"),1)])]),e("section",null,[e("h3",null,[d(o(at),{size:16}),t[10]||(t[10]=u(" Funcionario(s) Titular(es)",-1))]),(g=o(n).detalle.funcionarios)!=null&&g.length?(r(),c("div",dt,[(r(!0),c(A,null,P(o(n).detalle.funcionarios,l=>(r(),c("div",{key:l.rutFuncionario,style:{"margin-bottom":"0.4rem"}},[d(p,{to:`/funcionarios/${l.rutFuncionario}`,style:{"font-weight":"500"}},{default:x(()=>[u(i(l.rutFormateado),1)]),_:2},1032,["to"]),e("span",rt,i(l.nombreCompleto||""),1)]))),128))])):(r(),c("p",ut,"Sin funcionarios asociados (sin retenciones activas)."))]),e("section",null,[e("h3",null,[d(o(et),{size:16}),t[11]||(t[11]=u(" Cuenta Bancaria",-1))]),e("dl",null,[t[12]||(t[12]=e("dt",null,"Banco",-1)),e("dd",null,i(o(n).detalle.nombreBanco||o(n).detalle.codBanco||"-"),1),t[13]||(t[13]=e("dt",null,"Tipo Cuenta",-1)),e("dd",null,i(w.value),1),t[14]||(t[14]=e("dt",null,"Cuenta",-1)),e("dd",null,i(o(G)(o(n).detalle.ctaEstado||o(n).detalle.ctaOtBanco)),1),t[15]||(t[15]=e("dt",null,"Sucursal",-1)),e("dd",null,i(o(n).detalle.sucursal||"-"),1)])])]),(v=o(n).detalle.retenciones)!=null&&v.length?(r(),c("section",ct,[e("h3",null,[d(o(H),{size:16}),t[16]||(t[16]=u(" Retenciones Activas",-1))]),e("table",mt,[t[17]||(t[17]=e("thead",null,[e("tr",null,[e("th",null,"Funcionario"),e("th",null,"Monto"),e("th",null,"Codigo"),e("th",null,"Tipo Pago"),e("th",null,"Periodo")])],-1)),e("tbody",null,[(r(!0),c(A,null,P(o(n).detalle.retenciones,l=>{var f;return r(),c("tr",{key:l.id},[e("td",null,i(l.nombreFuncionario||l.rutTitularFormateado),1),e("td",null,"$"+i((f=l.monto)==null?void 0:f.toLocaleString("es-CL")),1),e("td",null,i(l.codRetencion),1),e("td",null,i(l.tipoPago),1),e("td",null,i(l.periodoProceso),1)])}),128))])])])):$("",!0),e("div",pt,[d(p,{to:`/beneficiarios/${z.rut}/editar`,class:"btn btn-primary"},{default:x(()=>[d(o(ot),{size:16}),t[18]||(t[18]=u(" Editar ",-1))]),_:1},8,["to"]),e("button",{class:"btn btn-secondary",onClick:V},[d(o(Z),{size:16}),t[19]||(t[19]=u(" Imprimir Ficha ",-1))]),o(n).detalle.estado==="A"?(r(),c("button",{key:0,class:"btn btn-danger",onClick:t[1]||(t[1]=l=>C.value=!0)},[d(o(nt),{size:16}),t[20]||(t[20]=u(" Inactivar ",-1))])):$("",!0),d(p,{to:"/beneficiarios",class:"btn btn-secondary"},{default:x(()=>[d(o(E),{size:16}),t[21]||(t[21]=u(" Volver ",-1))]),_:1})]),d(Q,{modelValue:C.value,"onUpdate:modelValue":t[2]||(t[2]=l=>C.value=l),title:"Inactivar Beneficiario",message:`¿Esta seguro de inactivar a ${o(n).detalle.nombreBeneficiario}? Esta accion cambiara su estado a Inactivo.`,confirmText:"Si, inactivar",variant:"danger",onConfirm:D},null,8,["modelValue","message"])])):o(n).loading?(r(),c("div",ft,"Cargando datos del beneficiario...")):(r(),c("div",bt,[d(o(_),{size:48,class:"empty-icon"}),t[23]||(t[23]=e("p",null,"Beneficiario no encontrado",-1)),d(p,{to:"/beneficiarios",class:"btn btn-secondary",style:{"margin-top":"0.5rem"}},{default:x(()=>[d(o(E),{size:16}),t[22]||(t[22]=u(" Volver a la lista ",-1))]),_:1})]))}}};export{Ft as default};
