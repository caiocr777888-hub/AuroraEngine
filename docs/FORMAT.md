# Aurora project / scene — versão 1

Arquivos JSON UTF-8, escritos por DataContractJsonSerializer. Nomes de propriedades têm maiúsculas significativas. `Root` é informação derivada do caminho do projeto e não é serializado.

`project.aurora`:

```json
{"Version":1,"Name":"My Game","Template":"3D","StartupScene":"Scenes/Main.scene"}
```

`Scenes/Main.scene` contém `Version: 1`, `Name` e `Entities` (array). Cada entidade tem `Id` (GUID string), `Parent` (ID ou null), `Name`, `Shape`, `Visible`, `X/Y/Z`, `RX/RY/RZ` em graus, `SX/SY/SZ`, `Color` e `Components`. Transformações são locais, aplicadas na ordem escala, rotação X/Y/Z, translação, transformação do pai. Unidades espaciais são convencionais; a gravidade usa 9.81 unidades/s².

Componente: `{"Type":"Spin","Value":45}`. Tipos executados: Spin, Controller, Gravity. Tipos desconhecidos são preservados, mas não executados. Sem IDs próprios para componentes na versão 1.

O carregador rejeita versões diferentes de 1, referências ausentes, ciclos, IDs duplicados, números não finitos e listas com mais de 10.000 entidades. O caminho da cena deve ficar dentro do projeto. Não é um formato seguro para conteúdo hostil: não abrir arquivos de origem não confiável.

Migrações futuras devem: ler Version antes de converter, manter cópia original, migrar sequencialmente vN → vN+1 e validar antes de substituir. Nenhuma migração é necessária ou implementada para v1. Versões futuras não são sobrescritas silenciosamente.

Salvamento escreve `.tmp` no mesmo diretório e usa File.Replace para substituir o arquivo existente com backup `.bak`. Primeiro salvamento usa File.Move. Uma falha de escrita preserva o arquivo anterior. Backup pode ser restaurado manualmente após fechar o editor.
