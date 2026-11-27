// 디자인 캔버스 보드를 브라우저에서 그대로 돌리기 위한 작은 런타임.
// 템플릿 문법: {{a.b}} 값 넣기, <sc-for list as>, <sc-if value>, <dc-import name>, onClick="{{fn}}"
(function () {
  'use strict';

  class DCLogic {
    constructor(props) {
      this.props = props || {};
      this.state = {};
    }
    setState(patch) {
      Object.assign(this.state, typeof patch === 'function' ? patch(this.state) : patch);
      if (this.__render) this.__render();
    }
  }

  const registry = {};
  const SVG_NS = 'http://www.w3.org/2000/svg';
  const WHOLE = /^\s*\{\{\s*([^}]+?)\s*\}\}\s*$/;
  const PART = /\{\{\s*([^}]+?)\s*\}\}/g;

  function lookup(path, scope) {
    path = path.trim();
    if (path === 'true') return true;
    if (path === 'false') return false;
    if (path === 'null') return null;
    if (/^-?\d+(\.\d+)?$/.test(path)) return Number(path);
    const parts = path.split('.');
    let cur = scope;
    for (const p of parts) {
      if (cur == null) return undefined;
      cur = cur[p];
    }
    return cur;
  }

  function evalAttr(value, scope) {
    const m = value.match(WHOLE);
    if (m) return lookup(m[1], scope);
    return value.replace(PART, (_, p) => {
      const v = lookup(p, scope);
      return v == null ? '' : String(v);
    });
  }

  function camel(name) {
    return name.replace(/-([a-z])/g, (_, c) => c.toUpperCase());
  }

  function register(name, templateHtml, Cls, defaults) {
    const tpl = document.createElement('template');
    tpl.innerHTML = templateHtml;
    registry[name] = { tpl, Cls, defaults: defaults || {} };
  }

  function mount(host, name, props) {
    const entry = registry[name];
    if (!entry) throw new Error('등록되지 않은 컴포넌트: ' + name);
    const inst = new entry.Cls(Object.assign({}, entry.defaults, props));
    inst.__children = [];
    attach(inst, name, host);
    inst.__render();
    if (inst.componentDidMount) inst.componentDidMount();
    return inst;
  }

  // inst가 host 안에 그려지도록 연결한다 (부모가 다시 그려지면 새 host로 바뀐다)
  function attach(inst, name, host) {
    const entry = registry[name];
    inst.__render = function () {
      const vals = Object.assign({}, inst.props, inst.renderVals ? inst.renderVals() : {});
      const ctx = { inst, importIndex: 0 };
      const frag = document.createDocumentFragment();
      for (const n of entry.tpl.content.childNodes) renderNode(n, vals, frag, ctx, false);
      const times = saveAnimTimes(host);
      host.replaceChildren(frag);
      restoreAnimTimes(host, times);
    };
  }

  // 다시 그릴 때마다 DOM을 새로 만들기 때문에, 그대로 두면 CSS 애니메이션이 매번 처음부터 다시 시작한다.
  // 같은 자리에 같은 애니메이션이 다시 생기면 진행 시간을 이어 붙인다.
  function animKey(el, host, name) {
    const path = [];
    for (let n = el; n && n !== host; n = n.parentElement) {
      let i = 0;
      for (let s = n.previousElementSibling; s; s = s.previousElementSibling) i++;
      path.push(n.tagName + '.' + n.getAttribute('class') + '#' + i);
    }
    return path.reverse().join('>') + '@' + name;
  }

  function cssAnims(host) {
    if (!host.getAnimations) return [];
    return host.getAnimations({ subtree: true }).filter(a => a.animationName && a.effect && a.effect.target);
  }

  function saveAnimTimes(host) {
    const m = new Map();
    for (const a of cssAnims(host)) m.set(animKey(a.effect.target, host, a.animationName), a.currentTime);
    return m;
  }

  function restoreAnimTimes(host, times) {
    if (!times.size) return;
    for (const a of cssAnims(host)) {
      const t = times.get(animKey(a.effect.target, host, a.animationName));
      if (t != null) a.currentTime = t;
    }
  }

  function renderChildren(node, scope, out, ctx, svg) {
    for (const c of node.childNodes) renderNode(c, scope, out, ctx, svg);
  }

  function renderNode(node, scope, out, ctx, svg) {
    if (node.nodeType === Node.TEXT_NODE) {
      out.appendChild(document.createTextNode(evalAttr(node.nodeValue, scope) ?? ''));
      return;
    }
    if (node.nodeType !== Node.ELEMENT_NODE) return;
    const tag = node.localName;

    if (tag === 'sc-for') {
      const list = evalAttr(node.getAttribute('list') || '', scope) || [];
      const as = node.getAttribute('as') || 'item';
      list.forEach((item, i) => {
        const child = Object.create(scope);
        child[as] = item;
        child.$index = i;
        renderChildren(node, child, out, ctx, svg);
      });
      return;
    }
    if (tag === 'sc-if') {
      if (evalAttr(node.getAttribute('value') || '', scope)) renderChildren(node, scope, out, ctx, svg);
      return;
    }
    if (tag === 'dc-import') {
      const name = node.getAttribute('name');
      const props = {};
      for (const a of node.attributes) {
        if (a.name === 'name' || a.name.startsWith('hint-')) continue;
        props[camel(a.name)] = evalAttr(a.value, scope);
      }
      const holder = document.createElement('div');
      holder.style.display = 'contents';
      const idx = ctx.importIndex++;
      const child = ctx.inst.__children[idx];
      if (!child) {
        ctx.inst.__children[idx] = mount(holder, name, props);
      } else {
        // 이미 있는 자식은 상태를 유지한 채 새 자리에 다시 그린다
        child.props = Object.assign({}, registry[name].defaults, props);
        attach(child, name, holder);
        child.__render();
      }
      out.appendChild(holder);
      return;
    }

    const inSvg = svg || tag === 'svg';
    const el = inSvg ? document.createElementNS(SVG_NS, tag) : document.createElement(tag);
    for (const a of node.attributes) {
      if (a.name.startsWith('hint-')) continue;
      if (/^on[a-z]+$/.test(a.name)) {
        const fn = evalAttr(a.value, scope);
        if (typeof fn === 'function') el.addEventListener(a.name.slice(2), fn);
        continue;
      }
      const v = evalAttr(a.value, scope);
      if (v === false || v == null) continue;
      el.setAttribute(a.name, v === true ? '' : String(v));
    }
    renderChildren(node, scope, el, ctx, inSvg && tag !== 'foreignObject');
    out.appendChild(el);
  }

  window.DCLogic = DCLogic;
  window.DC = { register, mount };
})();
