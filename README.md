# Last Minute Student

Jogo desenvolvido no âmbito da unidade curricular **Tecnologias Multimédia** da Universidade.

---

## Grupo

| Nome           | Número de Aluno |
|----------------|-----------------|
| Tiago Barros   | 32169           |
| Miguel Pereira | 32168           |

---

## Versão do Unity

**Unity 6000.3.9f1** (versão LTS pedida no enunciado — sem documentação adicional necessária).

---

## Descrição do Jogo

**Last Minute Student** é um *endless runner* 3D em perspetiva na terceira pessoa. O jogador controla um estudante que corre para chegar à aula a tempo, enquanto é perseguido pelo professor. O objetivo é sobreviver o máximo de tempo possível, desviando de obstáculos e recolhendo colecionáveis que conferem pontos e poderes.

### Funcionalidades Implementadas

- **Corrida infinita** com geração procedural de *chunks* de 30 unidades, reciclados à medida que o jogador avança.
- **Sistema de 3 faixas** com mudança lateral fluida (espaçamento de 2,5 unidades).
- **Salto** com duplo salto opcional (via power-up), com física personalizada (velocidade inicial 12 m/s, gravidade 26 m/s²).
- **Perseguidor (IA do professor)** com gestão de distância dinâmica: o professor aproxima-se ao longo do tempo e desacelera quando o jogador acerta num obstáculo; ao atingir o jogador, despoleta uma cinemática de apanha.
- **Obstáculos variados**: barreiras, cones de trânsito, bancos, caixotes, mesas, computadores, cacifos, entre outros.
- **Colecionáveis**: livros/moedas (pontos), *Speed Boost* (velocidade ×1,5 durante 5 s), *Double Jump* (salto duplo).
- **Dificuldade progressiva**: a velocidade aumenta de 18 m/s até 28 m/s; a probabilidade de spawn de obstáculos escala com a pontuação (40 % → 100 %).
- **Barra de proximidade** no HUD que indica o perigo de ser apanhado (verde → vermelho).
- **Câmara com 3 presets** (padrão, lateral, cinemática) cicláveis em jogo; evitação de paredes por *sphere cast*.
- **Audio Manager** com sons para moeda, obstáculo, *stumble*, *speed boost*, *game over*, captura, clique UI, início e salto.
- **Efeitos de partículas** no ponto de impacto com limpeza automática ao fim de 3 s.
- **UI totalmente procedural** (sem prefabs de UI): menu principal, HUD, ecrã de *game over* e painel de definições — tudo gerado em código.
- **Object Pooling** para prefabs de spawn, evitando alocações repetidas no GC.
- **Tabelas de spawn** em ScriptableObject com pesos, restrições de faixa e *gating* por pontuação.
- **Sistema de definições** persistente: volume SFX e volume de música.

---

## Jogabilidade

### Objetivo
Correr o maior tempo/distância possível sem ser apanhado pelo professor. A pontuação aumenta com o tempo de sobrevivência e com os livros recolhidos.

### Controlos

| Ação | Tecla |
|------|-------|
| Mover para a esquerda | `A` ou `←` |
| Mover para a direita | `D` ou `→` |
| Saltar | `Espaço` |
| Ciclar câmara | `V` |

> O jogo suporta também gamepad (Unity Input System).

### Regras
- O jogador perde se o bob o alcançar (distância < 1,35 unidades → cinemática de fim).
- O jogador também perde se cair para Y < -3 (fora da faixa).
- Ao colidir com um obstáculo, ocorre um *stumble* (~1 s de vulnerabilidade) e o professor recupera terreno.
- Máximo de 1 obstáculo por linha de spawn; período de graça no início reduz a densidade.

---

## Como Abrir o Projeto

### Pré-requisitos
- **Unity Hub** instalado ([download](https://unity.com/download)).
- Módulo **Unity 6000.3.9f1** instalado através do Unity Hub (separador *Installs*).

### Passos
1. Clonar ou descarregar o repositório:
   ```
   git clone https://github.com/PMiguelP/LastMinuteStudent.git
   ```
2. Abrir o **Unity Hub**.
3. Clicar em **Add** → **Add project from disk** e selecionar a pasta `LastMinuteStudent`.
4. Aguardar que o Unity importe os assets (primeira abertura pode demorar alguns minutos).
5. No painel *Project*, abrir a cena principal em `Assets/Scenes/SampleScene.unity`.
6. Carregar em **Play** (▶) para correr o jogo.

> **Nota:** O projeto usa **Git LFS** para ficheiros `.fbx`. Se os modelos aparecerem como ponteiros de texto, instalar o [Git LFS](https://git-lfs.github.com/) e correr `git lfs pull`.

---

## Assets Multimédia

### Modelos 3D (`.fbx`)
| Asset | Origem | Resolução / Tamanho | Justificação |
|-------|--------|-------------------|--------------|
| Personagem jogador (`Aj@*.fbx`) | Mixamo (free) | Baixa-média poligonagem | Animações de corrida, salto e *stumble* prontas |
| Personagem professor (`Ch17_nonPBR@Fast Run.fbx`) | Mixamo (free) | Baixa-média poligonagem | Distingue visualmente o perseguidor |
| Obstáculos de cidade | SimplePoly City, Kabungus School, ithappy Furniture | Low-poly | Estilo consistente; custo GPU baixo em *endless runner* |
| Estrada e passeio | BrokenVector Low Poly Road Pack | Low-poly | Coerência visual com o tema urbano |

### Texturas
| Textura | Formato | Dimensões | Justificação |
|---------|---------|-----------|--------------|
| Personagem jogador (`Boy01_diffuse.jpg`, `_normal`, `_spec`) | JPG | 1024 × 1024 | Qualidade suficiente; JPG reduz tamanho de ficheiro |
| Personagem professor (`Ch17_1001_Diffuse.png`) | PNG | 2048 × 2048 | Maior detalhe necessário pelo nível de zoom da câmara |
| Betão (`Concrete032_1K-JPG`) | JPG | 1024 × 1024 | PBR texturing para chão; resolução 1K adequada para runner |
| Estrada (`Road010A_1K-JPG`) | JPG | 1024 × 1024 | Idem |

### Áudio
| Clip | Formato | Justificação |
|------|---------|--------------|
| Música de fundo (`Assets/Gameplay/Audio/Music/background_Music.wav`) | WAV | Sem compressão para streaming contínuo sem *glitch* |
| Salto (`Assets/Gameplay/Audio/SFX/jump-sfx.wav`) | WAV | SFX curto; WAV para latência mínima no playback |
| SFX de UI e jogo (`Assets/Gameplay/Audio/SFX/DM-CGS-*.wav`) | WAV | Pack licenciado livre de royalties; WAV para qualidade |

### Efeitos de Partículas
- **Cartoon FX Remaster** (JMO Assets) — efeitos de impacto e recolha de colecionáveis; estilo cartoon coerente com o visual low-poly do jogo.

---

## Observações e Limitações

- A cena principal chama-se `SampleScene` (nome do template URP original mantido por conveniência).
- Foi realizada uma limpeza conservadora de conteúdo legado: cenas demo isoladas e ficheiros `.unitypackage` de upgrade/legacy foram removidos para reduzir peso do repositório sem impacto no runtime.
- A pasta `Assets/_Recovery/` contém uma cena de recuperação temporária que não é incluída no *build*.
