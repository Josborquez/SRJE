import{i as at,_ as lt,n as it,p as st,I as X,d as a,o as s,c as d,a as e,b as c,k as p,t as i,x as R,f as B,J as dt,G as rt,y as k,z as $,S as ut,w as ct,e as w,A as V,v as q,j as P,B as mt,g as C,E as J,m as pt,h as vt}from"./index-CD4b6Z7x.js";import{f as D}from"./useFormato-D2DftH4l.js";import{A as bt,C as ft}from"./AlertMessage-BFIFTm-D.js";import{U as Ct,P as gt,A as Y,a as yt}from"./user-x-iH0S_2Ix.js";import{C as ht}from"./circle-check-CSC9rY5R.js";import{L as K}from"./landmark-D5M1CaIa.js";import{P as Q}from"./pencil-Dl4q7Szu.js";import{B as xt}from"./ban-BBYsQTxb.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const Bt=at("briefcase",[["path",{d:"M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16",key:"jecpp"}],["rect",{width:"20",height:"14",x:"2",y:"6",rx:"2",key:"i6l2r4"}]]),kt={key:0,class:"detalle-beneficiario"},$t={class:"page-header"},zt={class:"sections"},wt={key:0},Ft={style:{"margin-left":"0.5rem",color:"var(--text-secondary)"}},Tt={key:1,style:{color:"var(--text-muted)","font-size":"0.85rem"}},At={key:0},Pt={class:"data-table"},St={key:1,class:"card-section"},Mt={class:"data-table"},Et=["onClick"],Lt={class:"modal-box"},Rt={style:{color:"var(--text-secondary)","font-size":"0.85rem","margin-bottom":"1rem"}},Vt={key:0,class:"form-group"},Dt=["value"],It={key:1,class:"campos-manuales"},Ut={class:"form-group"},Nt=["value"],jt={class:"form-group"},Ht=["value"],Gt={class:"form-group"},Ot={key:2,class:"cuenta-preview"},Xt={class:"cuenta-badge"},qt={class:"form-group"},Jt={class:"modal-actions"},Yt=["disabled"],Kt={class:"actions"},Qt={key:1,class:"loading"},Wt={key:2,class:"empty-state"},Zt={__name:"DetalleBeneficiario",props:{rut:{type:[String,Number],required:!0}},setup(I){const S=I,l=it(),W=vt(),g=C(""),z=C("info"),M=C(!1),y=C(null),v=C({monto:0,codBanco:null,tipoCuenta:null,numeroCuenta:""}),f=C(""),F=C(!1),U=C([]),N=C([]),Z=J(()=>{var t,m;if(!f.value)return"";const n=(m=(t=l.detalle)==null?void 0:t.cuentas)==null?void 0:m.find(b=>b.id===f.value);return n?`${n.nombreBanco||n.codBanco} — ${n.tipoCuentaDescripcion||""} — ${n.numeroCuenta}`:""}),j=J(()=>{var t;const n=(t=l.detalle)==null?void 0:t.tipoCuenta;return n===1?"Cuenta Corriente":n===2?"Ahorro / CuentaRUT":n===3?"Cuenta Vista":"-"});st(async()=>{l.detalle=null,l.obtener(Number(S.rut));try{const[n,t]=await Promise.all([X.bancos(),X.tiposCuenta()]);U.value=n.data,N.value=t.data}catch{}});function _(n){var b;y.value=n,v.value={monto:n.monto,codBanco:n.codBanco??null,tipoCuenta:n.tipoCuenta??null,numeroCuenta:n.numeroCuenta??""};const m=(((b=l.detalle)==null?void 0:b.cuentas)||[]).find(h=>h.codBanco===n.codBanco&&h.numeroCuenta===n.numeroCuenta);f.value=m?m.id:""}function tt(){var t,m;if(!f.value)return;const n=(m=(t=l.detalle)==null?void 0:t.cuentas)==null?void 0:m.find(b=>b.id===f.value);n&&(v.value.codBanco=n.codBanco,v.value.tipoCuenta=n.tipoCuenta,v.value.numeroCuenta=n.numeroCuenta)}function E(){y.value=null,f.value=""}async function et(){F.value=!0;try{await l.actualizarRetencion(Number(S.rut),y.value.id,v.value),E(),z.value="success",g.value="Retencion actualizada exitosamente."}catch{z.value="error",g.value=l.error||"Error al actualizar la retencion."}finally{F.value=!1}}function r(n){const t=document.createElement("div");return t.textContent=n??"",t.innerHTML}function ot(){var H,G,O;const n=l.detalle,t=new Date().toLocaleDateString("es-CL"),m=n.sexo==="M"?"Masculino":n.sexo==="F"?"Femenino":"-",b=j.value,h=n.ctaEstado||n.ctaOtBanco||"-",T=n.nombreBanco||n.codBanco||"-";let x="";(H=n.funcionarios)!=null&&H.length?x=n.funcionarios.map(u=>`<div style="margin-bottom: 3px;"><strong>${r(u.rutFormateado)}</strong> <span style="color: #64748b; margin-left: 6px;">${r(u.nombreCompleto)}</span></div>`).join(""):x='<p class="muted">Sin funcionarios asociados.</p>';let o="";(G=n.retenciones)!=null&&G.length?o=`<table class="ret-table">
      <thead><tr><th>Funcionario</th><th>Banco</th><th>Cuenta</th><th>Monto</th><th>Codigo</th><th>Tipo Pago</th><th>Periodo</th></tr></thead>
      <tbody>${n.retenciones.map(u=>`<tr>
        <td>${r(u.nombreFuncionario||u.rutTitularFormateado||"-")}</td>
        <td>${r(u.nombreBanco||"-")}</td>
        <td>${r(u.numeroCuenta||"-")}</td>
        <td>$${(u.monto||0).toLocaleString("es-CL")}</td>
        <td>${r(u.codRetencion||"-")}</td>
        <td>${r(u.tipoPago||"-")}</td>
        <td>${r(u.periodoProceso||"-")}</td>
      </tr>`).join("")}</tbody></table>`:o='<p class="muted">Sin retenciones activas.</p>';const A=`<!DOCTYPE html><html><head><meta charset="utf-8">
<title>Ficha Beneficiario - ${n.rutFormateado}</title>
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
<h1>${r(n.nombreBeneficiario)}</h1>
<p class="subtitle">RUT: ${r(n.rutFormateado)} | <span class="estado ${n.estado==="A"?"estado-a":"estado-i"}">${n.estado==="A"?"Activo":"Inactivo"}</span></p>

<div class="section">
  <h2>Datos Personales</h2>
  <dl>
    <dt>Sexo</dt><dd>${r(m)}</dd>
    <dt>Estado Civil</dt><dd>${r(n.estadoCivil||"-")}</dd>
    <dt>Domicilio</dt><dd>${r(n.domicilio||"-")}</dd>
    <dt>Comuna</dt><dd>${r(n.comuna||"-")}</dd>
    <dt>Telefono</dt><dd>${r(n.telefono||"-")}</dd>
  </dl>
</div>

<div class="section">
  <h2>Funcionario(s) Titular(es)</h2>
  ${x}
</div>

<div class="section">
  <h2>Cuenta Bancaria Principal</h2>
  <dl>
    <dt>Banco</dt><dd>${r(T)}</dd>
    <dt>Tipo Cuenta</dt><dd>${r(b)}</dd>
    <dt>Cuenta</dt><dd>${r(h)}</dd>
    <dt>Sucursal</dt><dd>${r(n.sucursal||"-")}</dd>
  </dl>
</div>

${(O=n.cuentas)!=null&&O.length?`<div class="section">
  <h2>Cuentas Almacenadas (${n.cuentas.length})</h2>
  <table class="ret-table">
    <thead><tr><th>Banco</th><th>Tipo Cuenta</th><th>N° Cuenta</th><th>Alias</th></tr></thead>
    <tbody>${n.cuentas.map(u=>`<tr>
      <td>${r(u.nombreBanco||String(u.codBanco))}</td>
      <td>${r(u.tipoCuentaDescripcion||String(u.tipoCuenta))}</td>
      <td>${r(u.numeroCuenta||"-")}</td>
      <td>${r(u.alias||"-")}</td>
    </tr>`).join("")}</tbody>
  </table>
</div>`:""}

<div class="section">
  <h2>Retenciones Activas</h2>
  ${o}
</div>

<script>window.onload = function() { window.print(); window.close(); }<\/script>
</body></html>`,L=window.open("","_blank","width=800,height=600");L.document.write(A),L.document.close()}async function nt(){await l.inactivar(Number(S.rut))?(z.value="success",g.value="Beneficiario inactivado exitosamente. Redirigiendo...",setTimeout(()=>W.push("/beneficiarios"),1500)):(z.value="error",g.value=l.error||"Error al inactivar el beneficiario.")}return(n,t)=>{var b,h,T,x;const m=pt("router-link");return a(l).detalle?(s(),d("div",kt,[e("div",$t,[e("h1",null,[c(a(Ct),{size:24}),p(" "+i(a(l).detalle.nombreBeneficiario),1)]),e("p",null,i(a(l).detalle.rutFormateado),1)]),g.value?(s(),R(bt,{key:0,message:g.value,type:z.value,onClose:t[0]||(t[0]=o=>g.value="")},null,8,["message","type"])):B("",!0),e("div",zt,[e("section",null,[e("h3",null,[c(a(dt),{size:16}),t[8]||(t[8]=p(" Datos Personales",-1))]),e("dl",null,[t[9]||(t[9]=e("dt",null,"Estado",-1)),e("dd",null,[e("span",{class:rt("estado estado-"+a(l).detalle.estado.toLowerCase())},[a(l).detalle.estado==="A"?(s(),R(a(ht),{key:0,size:13})):(s(),R(a(ft),{key:1,size:13})),p(" "+i(a(l).detalle.estado==="A"?"Activo":"Inactivo"),1)],2)]),t[10]||(t[10]=e("dt",null,"Sexo",-1)),e("dd",null,i(a(l).detalle.sexo==="M"?"Masculino":a(l).detalle.sexo==="F"?"Femenino":"-"),1),t[11]||(t[11]=e("dt",null,"Estado Civil",-1)),e("dd",null,i(a(l).detalle.estadoCivil||"-"),1),t[12]||(t[12]=e("dt",null,"Domicilio",-1)),e("dd",null,i(a(l).detalle.domicilio||"-"),1),t[13]||(t[13]=e("dt",null,"Comuna",-1)),e("dd",null,i(a(l).detalle.comuna||"-"),1),t[14]||(t[14]=e("dt",null,"Telefono",-1)),e("dd",null,i(a(l).detalle.telefono||"-"),1)])]),e("section",null,[e("h3",null,[c(a(Bt),{size:16}),t[15]||(t[15]=p(" Funcionario(s) Titular(es)",-1))]),(b=a(l).detalle.funcionarios)!=null&&b.length?(s(),d("div",wt,[(s(!0),d(k,null,$(a(l).detalle.funcionarios,o=>(s(),d("div",{key:o.rutFuncionario,style:{"margin-bottom":"0.4rem"}},[c(m,{to:`/funcionarios/${o.rutFuncionario}`,style:{"font-weight":"500"}},{default:P(()=>[p(i(o.rutFormateado),1)]),_:2},1032,["to"]),e("span",Ft,i(o.nombreCompleto||""),1)]))),128))])):(s(),d("p",Tt,"Sin funcionarios asociados (sin retenciones activas)."))]),e("section",null,[e("h3",null,[c(a(K),{size:16}),t[16]||(t[16]=p(" Cuenta Bancaria Principal",-1))]),e("dl",null,[t[17]||(t[17]=e("dt",null,"Banco",-1)),e("dd",null,i(a(l).detalle.nombreBanco||a(l).detalle.codBanco||"-"),1),t[18]||(t[18]=e("dt",null,"Tipo Cuenta",-1)),e("dd",null,i(j.value),1),t[19]||(t[19]=e("dt",null,"Cuenta",-1)),e("dd",null,i(a(D)(a(l).detalle.ctaEstado||a(l).detalle.ctaOtBanco)),1),t[20]||(t[20]=e("dt",null,"Sucursal",-1)),e("dd",null,i(a(l).detalle.sucursal||"-"),1)])]),(h=a(l).detalle.cuentas)!=null&&h.length?(s(),d("section",At,[e("h3",null,[c(a(K),{size:16}),p(" Cuentas Almacenadas ("+i(a(l).detalle.cuentas.length)+")",1)]),e("table",Pt,[t[21]||(t[21]=e("thead",null,[e("tr",null,[e("th",null,"Banco"),e("th",null,"Tipo Cuenta"),e("th",null,"N° Cuenta"),e("th",null,"Alias")])],-1)),e("tbody",null,[(s(!0),d(k,null,$(a(l).detalle.cuentas,o=>(s(),d("tr",{key:o.id},[e("td",null,i(o.nombreBanco||o.codBanco),1),e("td",null,i(o.tipoCuentaDescripcion||o.tipoCuenta),1),e("td",null,i(a(D)(o.numeroCuenta)),1),e("td",null,i(o.alias||"-"),1)]))),128))])])])):B("",!0)]),(T=a(l).detalle.retenciones)!=null&&T.length?(s(),d("section",St,[e("h3",null,[c(a(ut),{size:16}),t[22]||(t[22]=p(" Retenciones Activas",-1))]),e("table",Mt,[t[24]||(t[24]=e("thead",null,[e("tr",null,[e("th",null,"Funcionario"),e("th",null,"Banco"),e("th",null,"Cuenta"),e("th",null,"Monto"),e("th",null,"Codigo"),e("th",null,"Tipo Pago"),e("th",null,"Periodo"),e("th",null,"Acciones")])],-1)),e("tbody",null,[(s(!0),d(k,null,$(a(l).detalle.retenciones,o=>{var A;return s(),d("tr",{key:o.id},[e("td",null,i(o.nombreFuncionario||o.rutTitularFormateado),1),e("td",null,i(o.nombreBanco||"-"),1),e("td",null,i(a(D)(o.numeroCuenta)),1),e("td",null,"$"+i((A=o.monto)==null?void 0:A.toLocaleString("es-CL")),1),e("td",null,i(o.codRetencion),1),e("td",null,i(o.tipoPago),1),e("td",null,i(o.periodoProceso),1),e("td",null,[e("button",{class:"btn-sm",onClick:L=>_(o)},[c(a(Q),{size:14}),t[23]||(t[23]=p(" Editar ",-1))],8,Et)])])}),128))])])])):B("",!0),y.value?(s(),d("div",{key:2,class:"modal-overlay",onClick:ct(E,["self"])},[e("div",Lt,[t[33]||(t[33]=e("h3",null,"Editar Retencion",-1)),e("p",Rt," Funcionario: "+i(y.value.nombreFuncionario||y.value.rutTitularFormateado)+" — Periodo: "+i(y.value.periodoProceso),1),(x=a(l).detalle.cuentas)!=null&&x.length?(s(),d("div",Vt,[t[26]||(t[26]=e("label",null,"Cuenta almacenada",-1)),w(e("select",{"onUpdate:modelValue":t[1]||(t[1]=o=>f.value=o),onChange:tt,class:"form-control cuenta-select"},[t[25]||(t[25]=e("option",{value:""},"— Ingresar manualmente —",-1)),(s(!0),d(k,null,$(a(l).detalle.cuentas,o=>(s(),d("option",{key:o.id,value:o.id},i(o.nombreBanco||o.codBanco)+" — "+i(o.tipoCuentaDescripcion||"")+" — "+i(o.numeroCuenta)+" "+i(o.alias?`(${o.alias})`:""),9,Dt))),128))],544),[[V,f.value]])])):B("",!0),f.value?(s(),d("div",Ot,[e("span",Xt,i(Z.value),1)])):(s(),d("div",It,[e("div",Ut,[t[28]||(t[28]=e("label",null,"Banco",-1)),w(e("select",{"onUpdate:modelValue":t[2]||(t[2]=o=>v.value.codBanco=o),class:"form-control"},[t[27]||(t[27]=e("option",{value:null},"— Sin banco —",-1)),(s(!0),d(k,null,$(U.value,o=>(s(),d("option",{key:o.codBanco,value:o.codBanco},i(o.nombreBanco),9,Nt))),128))],512),[[V,v.value.codBanco]])]),e("div",jt,[t[30]||(t[30]=e("label",null,"Tipo Cuenta",-1)),w(e("select",{"onUpdate:modelValue":t[3]||(t[3]=o=>v.value.tipoCuenta=o),class:"form-control"},[t[29]||(t[29]=e("option",{value:null},"—",-1)),(s(!0),d(k,null,$(N.value,o=>(s(),d("option",{key:o.codigo,value:o.codigo},i(o.descripcion),9,Ht))),128))],512),[[V,v.value.tipoCuenta]])]),e("div",Gt,[t[31]||(t[31]=e("label",null,"N° Cuenta",-1)),w(e("input",{"onUpdate:modelValue":t[4]||(t[4]=o=>v.value.numeroCuenta=o),class:"form-control",maxlength:"15"},null,512),[[q,v.value.numeroCuenta]])])])),e("div",qt,[t[32]||(t[32]=e("label",null,"Monto ($)",-1)),w(e("input",{"onUpdate:modelValue":t[5]||(t[5]=o=>v.value.monto=o),type:"number",min:"0",class:"form-control"},null,512),[[q,v.value.monto,void 0,{number:!0}]])]),e("div",Jt,[e("button",{class:"btn btn-primary",onClick:et,disabled:F.value},i(F.value?"Guardando...":"Guardar"),9,Yt),e("button",{class:"btn btn-secondary",onClick:E},"Cancelar")])])])):B("",!0),e("div",Kt,[c(m,{to:`/beneficiarios/${I.rut}/editar`,class:"btn btn-primary"},{default:P(()=>[c(a(Q),{size:16}),t[34]||(t[34]=p(" Editar ",-1))]),_:1},8,["to"]),e("button",{class:"btn btn-secondary",onClick:ot},[c(a(gt),{size:16}),t[35]||(t[35]=p(" Imprimir Ficha ",-1))]),a(l).detalle.estado==="A"?(s(),d("button",{key:0,class:"btn btn-danger",onClick:t[6]||(t[6]=o=>M.value=!0)},[c(a(xt),{size:16}),t[36]||(t[36]=p(" Inactivar ",-1))])):B("",!0),c(m,{to:"/beneficiarios",class:"btn btn-secondary"},{default:P(()=>[c(a(Y),{size:16}),t[37]||(t[37]=p(" Volver ",-1))]),_:1})]),c(mt,{modelValue:M.value,"onUpdate:modelValue":t[7]||(t[7]=o=>M.value=o),title:"Inactivar Beneficiario",message:`¿Esta seguro de inactivar a ${a(l).detalle.nombreBeneficiario}? Esta accion cambiara su estado a Inactivo.`,confirmText:"Si, inactivar",variant:"danger",onConfirm:nt},null,8,["modelValue","message"])])):a(l).loading?(s(),d("div",Qt,"Cargando datos del beneficiario...")):(s(),d("div",Wt,[c(a(yt),{size:48,class:"empty-icon"}),t[39]||(t[39]=e("p",null,"Beneficiario no encontrado",-1)),c(m,{to:"/beneficiarios",class:"btn btn-secondary",style:{"margin-top":"0.5rem"}},{default:P(()=>[c(a(Y),{size:16}),t[38]||(t[38]=p(" Volver a la lista ",-1))]),_:1})]))}}},se=lt(Zt,[["__scopeId","data-v-5d242fd1"]]);export{se as default};
